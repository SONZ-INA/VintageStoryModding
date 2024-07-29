namespace FoodShelves;

public static class InfoDisplay {
    public enum InfoDisplayOptions {
        ByBlock,
        ByShelf,
        BySegment,
        ByBlockAverageAndSoonest
    }

    public static void DisplayInfo(IPlayer forPlayer, StringBuilder sb, InventoryGeneric inv, InfoDisplayOptions displaySelection, int slotCount, int segmentsPerShelf, int itemsPerSegment, bool skipLine = true) {
        if (skipLine) sb.AppendLine(); // Space in between to be in line with vanilla

        ICoreAPI Api = inv.Api;

        if (displaySelection == InfoDisplayOptions.ByBlockAverageAndSoonest) {
            sb.AppendLine(PerishableInfoAverageAndSoonest(inv));
            return;
        }

        int selectedSegment = -1;
        if (forPlayer.CurrentBlockSelection != null)
            selectedSegment = forPlayer.CurrentBlockSelection.SelectionBoxIndex;

        if (displaySelection != InfoDisplayOptions.ByBlock && selectedSegment == -1) return;

        int start = 0, end = slotCount;

        switch (displaySelection) {
            case InfoDisplayOptions.ByBlock:
                start = slotCount - 1;
                end = -1;
                break;
            case InfoDisplayOptions.ByShelf:
                int selectedShelf = selectedSegment / segmentsPerShelf * (segmentsPerShelf * itemsPerSegment);
                start = selectedShelf;
                end = selectedShelf + (segmentsPerShelf * itemsPerSegment);
                break;
            case InfoDisplayOptions.BySegment:
                start = selectedSegment * itemsPerSegment;
                end = start + itemsPerSegment;
                break;
        }

        for (int i = start; i != end; i = displaySelection == InfoDisplayOptions.ByBlock ? i - 1 : i + 1) {
            if (inv[i].Empty) continue;

            ItemStack stack = inv[i].Itemstack;

            if (stack.Collectible.TransitionableProps != null &&
                stack.Collectible.TransitionableProps.Length > 0) {
                sb.Append(PerishableInfoCompact(Api, inv[i]));
            }
            else {
                sb.Append(stack.GetName());
                if (stack.StackSize > 1) 
                    sb.Append(" x" + stack.StackSize);
                
                sb.AppendLine();
            }
        }
    }

    private static string PerishableInfoCompact(ICoreAPI Api, ItemSlot contentSlot, bool withStackName = true) {
        if (contentSlot.Empty) return "";

        StringBuilder dsc = new();

        if (withStackName) {
            dsc.Append(contentSlot.Itemstack.GetName());
        }

        TransitionState[] transitionStates = contentSlot.Itemstack?.Collectible.UpdateAndGetTransitionStates(Api.World, contentSlot);

        if (transitionStates != null) {
            bool appendLine = false;

            for (int i = 0; i < transitionStates.Length; i++) {
                TransitionState state = transitionStates[i];
                TransitionableProperties prop = state.Props;
                float perishRate = contentSlot.Itemstack.Collectible.GetTransitionRateMul(Api.World, contentSlot, prop.Type);

                if (perishRate <= 0) continue;

                float transitionLevel = state.TransitionLevel;
                float freshHoursLeft = state.FreshHoursLeft / perishRate;

                appendLine = true;

                if (transitionLevel > 0) {
                    dsc.Append(", " + Lang.Get("{0}% spoiled", (int)Math.Round(transitionLevel * 100)));
                }
                else {
                    double hoursPerday = Api.World.Calendar.HoursPerDay;

                    if (freshHoursLeft / hoursPerday >= Api.World.Calendar.DaysPerYear) {
                        dsc.Append(", " + Lang.Get("fresh for {0} years", Math.Round(freshHoursLeft / hoursPerday / Api.World.Calendar.DaysPerYear, 1)));
                    }
                    else if (freshHoursLeft > hoursPerday) {
                        dsc.Append(", " + Lang.Get("fresh for {0} days", Math.Round(freshHoursLeft / hoursPerday, 1)));
                    }
                    else {
                        dsc.Append(", " + Lang.Get("fresh for {0} hours", Math.Round(freshHoursLeft, 1)));
                    }
                }
            }

            if (appendLine) dsc.AppendLine();
        }

        return dsc.ToString();
    }

    private static string PerishableInfoAverageAndSoonest(InventoryGeneric inv, bool withStackName = true) {
        if (inv == null || inv.Empty) return "";

        ICoreAPI Api = inv.Api;
        StringBuilder dsc = new();
        int nonRotItems = 0;
        int rotItems = 0;
        double totalFreshHours = 0;
        int itemCount = 0;
        ItemStack soonestPerishStack = null;
        double soonestPerishHours = double.MaxValue;
        float soonestTransitionLevel = 0;

        foreach (var slot in inv) {
            if (slot.Empty) continue;
            ItemStack stack = slot.Itemstack;

            if (stack.Item.Code.Path.StartsWith("rot")) {
                rotItems++;
                continue;
            }

            TransitionState[] transitionStates = stack?.Collectible.UpdateAndGetTransitionStates(Api.World, slot);

            if (transitionStates != null && transitionStates.Length > 0) {
                foreach (var state in transitionStates) {
                    double freshHoursLeft = state.FreshHoursLeft / stack.Collectible.GetTransitionRateMul(Api.World, slot, state.Props.Type);
                    if (state.Props.Type == EnumTransitionType.Perish) {
                        nonRotItems++;
                        totalFreshHours += freshHoursLeft * stack.StackSize;
                        itemCount += stack.StackSize;

                        if (freshHoursLeft < soonestPerishHours) {
                            soonestPerishHours = freshHoursLeft;
                            soonestPerishStack = stack;
                            soonestTransitionLevel = state.TransitionLevel;
                        }
                    }
                }
            }
        }

        // Number of items inside
        if (nonRotItems > 0) {
            dsc.AppendLine(Lang.Get("Fruits inside: {0}", nonRotItems));
        }

        // Number of rotten items
        if (rotItems > 0) {
            dsc.AppendLine(Lang.Get("Rotten fruits: {0}", rotItems));
        }

        // Average perish rate
        if (itemCount > 0) {
            double averageFreshHoursLeft = totalFreshHours / itemCount;
            double hoursPerday = Api.World.Calendar.HoursPerDay;

            if (averageFreshHoursLeft / hoursPerday >= Api.World.Calendar.DaysPerYear) {
                dsc.AppendLine(Lang.Get("Average perish rate: {0} years", Math.Round(averageFreshHoursLeft / hoursPerday / Api.World.Calendar.DaysPerYear, 1)));
            }
            else if (averageFreshHoursLeft > hoursPerday) {
                dsc.AppendLine(Lang.Get("Average perish rate: {0} days", Math.Round(averageFreshHoursLeft / hoursPerday, 1)));
            }
            else {
                dsc.AppendLine(Lang.Get("Average perish rate: {0} hours", Math.Round(averageFreshHoursLeft, 1)));
            }
        }

        // Item that will perish the soonest
        if (soonestPerishStack != null) {
            dsc.Append(withStackName ? Lang.Get("Soonest: ") + soonestPerishStack.GetName() : Lang.Get("Fruit"));
            double hoursPerday = Api.World.Calendar.HoursPerDay;

            if (soonestTransitionLevel > 0) {
                dsc.AppendLine(", " + Lang.Get("{0}% spoiled", (int)Math.Round(soonestTransitionLevel * 100)));
            }
            else {
                if (soonestPerishHours / hoursPerday >= Api.World.Calendar.DaysPerYear) {
                    dsc.AppendLine(", " + Lang.Get("will perish in {0} years", Math.Round(soonestPerishHours / hoursPerday / Api.World.Calendar.DaysPerYear, 1)));
                }
                else if (soonestPerishHours > hoursPerday) {
                    dsc.AppendLine(", " + Lang.Get("will perish in {0} days", Math.Round(soonestPerishHours / hoursPerday, 1)));
                }
                else {
                    dsc.AppendLine(", " + Lang.Get("will perish in {0} hours", Math.Round(soonestPerishHours, 1)));
                }
            }
        }
        else {
            dsc.AppendLine(Lang.Get("No fruit will perish soon."));
        }

        return dsc.ToString();
    }

    public static void PerishableInfoAverageAndSoonest(ItemStack[] contents, StringBuilder dsc, IWorldAccessor world) {
        if (contents == null) return;

        int fruitsCount = 0;
        int rotCount = 0;
        double totalFreshHours = 0;
        int itemCount = 0;
        ItemStack soonestPerishStack = null;
        double soonestPerishHours = double.MaxValue;
        float soonestTransitionLevel = 0;

        foreach (var stack in contents) {
            if (stack == null) continue;

            if (stack?.Collectible?.Code.Path.StartsWith("rot") == true) rotCount++;
            else fruitsCount++;

            ItemSlot tempSlot = new DummySlot(stack);
            TransitionState[] transitionStates = stack?.Collectible.UpdateAndGetTransitionStates(world, tempSlot);

            if (transitionStates != null && transitionStates.Length > 0) {
                foreach (var state in transitionStates) {
                    double freshHoursLeft = state.FreshHoursLeft / stack.Collectible.GetTransitionRateMul(world, tempSlot, state.Props.Type);
                    if (state.Props.Type == EnumTransitionType.Perish) {
                        totalFreshHours += freshHoursLeft * stack.StackSize;
                        itemCount += stack.StackSize;

                        if (freshHoursLeft < soonestPerishHours) {
                            soonestPerishHours = freshHoursLeft;
                            soonestPerishStack = stack;
                            soonestTransitionLevel = state.TransitionLevel;
                        }
                    }
                }
            }
        }

        // Number of fruits inside
        if (fruitsCount > 0) {
            dsc.AppendLine(Lang.Get("Fruits inside: {0}", fruitsCount));
        }

        // Number of rotten items
        if (rotCount > 0) {
            dsc.AppendLine(Lang.Get("Rotten fruits: {0}", rotCount));
        }

        // Average perish rate
        if (itemCount > 0) {
            double averageFreshHoursLeft = totalFreshHours / itemCount;
            double hoursPerday = world.Calendar.HoursPerDay;

            if (averageFreshHoursLeft / hoursPerday >= world.Calendar.DaysPerYear) {
                dsc.AppendLine(Lang.Get("Average perish rate: {0} years", Math.Round(averageFreshHoursLeft / hoursPerday / world.Calendar.DaysPerYear, 1)));
            }
            else if (averageFreshHoursLeft > hoursPerday) {
                dsc.AppendLine(Lang.Get("Average perish rate: {0} days", Math.Round(averageFreshHoursLeft / hoursPerday, 1)));
            }
            else {
                dsc.AppendLine(Lang.Get("Average perish rate: {0} hours", Math.Round(averageFreshHoursLeft, 1)));
            }
        }

        // Item that will perish the soonest
        if (soonestPerishStack != null) {
            dsc.Append(Lang.Get("Soonest: ") + soonestPerishStack.GetName());
            double hoursPerday = world.Calendar.HoursPerDay;

            if (soonestTransitionLevel > 0) {
                dsc.AppendLine(", " + Lang.Get("{0}% spoiled", (int)Math.Round(soonestTransitionLevel * 100)));
            }
            else {
                if (soonestPerishHours / hoursPerday >= world.Calendar.DaysPerYear) {
                    dsc.AppendLine(", " + Lang.Get("will perish in {0} years", Math.Round(soonestPerishHours / hoursPerday / world.Calendar.DaysPerYear, 1)));
                }
                else if (soonestPerishHours > hoursPerday) {
                    dsc.AppendLine(", " + Lang.Get("will perish in {0} days", Math.Round(soonestPerishHours / hoursPerday, 1)));
                }
                else {
                    dsc.AppendLine(", " + Lang.Get("will perish in {0} hours", Math.Round(soonestPerishHours, 1)));
                }
            }
        }
        else {
            dsc.AppendLine(Lang.Get("No fruit will perish soon."));
        }
    }
}
