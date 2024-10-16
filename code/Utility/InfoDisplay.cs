namespace FoodShelves;

public static class InfoDisplay {
    public enum InfoDisplayOptions {
        ByBlock,
        ByShelf,
        BySegment,
        ByBlockAverageAndSoonest
    }

    public static void DisplayInfo(IPlayer forPlayer, StringBuilder sb, InventoryGeneric inv, InfoDisplayOptions displaySelection, int slotCount, int segmentsPerShelf = 0, int itemsPerSegment = 0, bool skipLine = true) {
        if (skipLine) sb.AppendLine(); // Space in between to be in line with vanilla

        ICoreAPI Api = inv.Api;

        if (displaySelection == InfoDisplayOptions.ByBlockAverageAndSoonest) {
            List<ItemStack> itemStackList = new();
            foreach (var slot in inv) {
                ItemStack itemStack = slot.Itemstack;
                if (itemStack != null) itemStackList.Add(itemStack);
            }

            ItemStack[] contents = itemStackList.ToArray();
            PerishableInfoAverageAndSoonest(contents, sb, Api.World);
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
                int itemsPerShelf = segmentsPerShelf * itemsPerSegment;
                int selectedShelf = selectedSegment / segmentsPerShelf * itemsPerShelf;
                start = selectedShelf;
                end = selectedShelf + itemsPerShelf;
                break;
            case InfoDisplayOptions.BySegment:
                start = selectedSegment * itemsPerSegment;
                end = start + itemsPerSegment;
                break;
        }

        for (int i = start; i != end; i = displaySelection == InfoDisplayOptions.ByBlock ? i - 1 : i + 1) {
            if (i >= slotCount) break;
            if (inv[i].Empty) continue;

            ItemStack stack = inv[i].Itemstack;
            float ripenRate = stack.Collectible.GetTransitionRateMul(Api.World, inv[i], EnumTransitionType.Ripen); // Get ripen rate

            if (stack.Collectible.TransitionableProps != null &&
                stack.Collectible.TransitionableProps.Length > 0) {
                sb.Append(PerishableInfoCompact(Api, inv[i], ripenRate));
            }
            else {
                sb.Append(stack.GetName());
                if (stack.StackSize > 1) sb.Append(" x" + stack.StackSize);
                sb.AppendLine();
            }
        }
    }

    public static string PerishableInfoCompact(ICoreAPI Api, ItemSlot contentSlot, float ripenRate, bool withStackName = true) {
        if (contentSlot.Empty) return "";

        StringBuilder dsc = new();

        if (withStackName) {
            dsc.Append(contentSlot.Itemstack.GetName());
        }

        TransitionState[] transitionStates = contentSlot.Itemstack?.Collectible.UpdateAndGetTransitionStates(Api.World, contentSlot);

        bool nowSpoiling = false;

        if (transitionStates != null) {
            bool appendLine = false;
            for (int i = 0; i < transitionStates.Length; i++) {
                TransitionState state = transitionStates[i];
                TransitionableProperties prop = state.Props;
                float perishRate = contentSlot.Itemstack.Collectible.GetTransitionRateMul(Api.World, contentSlot, prop.Type);

                if (perishRate <= 0) continue;

                float transitionLevel = state.TransitionLevel;
                float freshHoursLeft = state.FreshHoursLeft / perishRate;

                switch (prop.Type) {
                    case EnumTransitionType.Perish:
                        appendLine = true;

                        if (transitionLevel > 0) {
                            nowSpoiling = true;
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
                        break;

                    case EnumTransitionType.Ripen:
                        if (nowSpoiling) break;

                        appendLine = true;

                        if (transitionLevel > 0) {
                            dsc.Append(", " + Lang.Get("{1:0.#} days left to ripen ({0}%)", (int)Math.Round(transitionLevel * 100), (state.TransitionHours - state.TransitionedHours) / Api.World.Calendar.HoursPerDay / ripenRate));
                        }
                        else {
                            double hoursPerday = Api.World.Calendar.HoursPerDay;

                            if (freshHoursLeft / hoursPerday >= Api.World.Calendar.DaysPerYear) {
                                dsc.Append(", " + Lang.Get("will ripen in {0} years", Math.Round(freshHoursLeft / hoursPerday / Api.World.Calendar.DaysPerYear, 1)));
                            }
                            else if (freshHoursLeft > hoursPerday) {
                                dsc.Append(", " + Lang.Get("will ripen in {0} days", Math.Round(freshHoursLeft / hoursPerday, 1)));
                            }
                            else {
                                dsc.Append(", " + Lang.Get("will ripen in {0} hours", Math.Round(freshHoursLeft, 1)));
                            }
                        }
                        break;
                }
            }

            if (appendLine) dsc.AppendLine();
        }

        return dsc.ToString();
    }

    public static void PerishableInfoAverageAndSoonest(ItemStack[] contents, StringBuilder dsc, IWorldAccessor world) {
        if (contents == null) return;

        int itemCount = 0;
        int rotCount = 0;
        double totalFreshHours = 0;
        int totalCount = 0;
        ItemStack soonestPerishStack = null;
        double soonestPerishHours = double.MaxValue;
        float soonestTransitionLevel = 0;

        foreach (var stack in contents) {
            if (stack == null) continue;

            if (stack?.Collectible?.Code.Path.StartsWith("rot") == true) rotCount++;
            else itemCount += stack.StackSize;

            ItemSlot tempSlot = new DummySlot(stack);
            TransitionState[] transitionStates = stack?.Collectible.UpdateAndGetTransitionStates(world, tempSlot);

            if (transitionStates != null && transitionStates.Length > 0) {
                foreach (var state in transitionStates) {
                    double freshHoursLeft = state.FreshHoursLeft / stack.Collectible.GetTransitionRateMul(world, tempSlot, state.Props.Type);
                    if (state.Props.Type == EnumTransitionType.Perish) {
                        totalFreshHours += freshHoursLeft * stack.StackSize;
                        totalCount += stack.StackSize;

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
        if (itemCount > 0) dsc.AppendLine(Lang.Get("foodshelves:Items inside {0}", itemCount));

        // Number of rotten items
        if (rotCount > 0) dsc.AppendLine(Lang.Get("Rotten Food: {0}", rotCount));

        // Average perish rate
        if (totalCount > 0) {
            double averageFreshHoursLeft = totalFreshHours / totalCount;
            double hoursPerday = world.Calendar.HoursPerDay;

            if (averageFreshHoursLeft / hoursPerday >= world.Calendar.DaysPerYear) {
                dsc.AppendLine(Lang.Get("foodshelves:Average perish rate {0} years", Math.Round(averageFreshHoursLeft / hoursPerday / world.Calendar.DaysPerYear, 1)));
            }
            else if (averageFreshHoursLeft > hoursPerday) {
                dsc.AppendLine(Lang.Get("foodshelves:Average perish rate {0} days", Math.Round(averageFreshHoursLeft / hoursPerday, 1)));
            }
            else {
                dsc.AppendLine(Lang.Get("foodshelves:Average perish rate {0} hours", Math.Round(averageFreshHoursLeft, 1)));
            }
        }

        // Item that will perish the soonest
        if (soonestPerishStack != null) {
            dsc.Append(Lang.Get("foodshelves:Soonest") + soonestPerishStack.GetName());
            double hoursPerday = world.Calendar.HoursPerDay;

            if (soonestTransitionLevel > 0) {
                dsc.AppendLine(", " + Lang.Get("{0}% spoiled", (int)Math.Round(soonestTransitionLevel * 100)));
            }
            else {
                if (soonestPerishHours / hoursPerday >= world.Calendar.DaysPerYear) {
                    dsc.AppendLine(", " + Lang.Get("foodshelves:will perish in {0} years", Math.Round(soonestPerishHours / hoursPerday / world.Calendar.DaysPerYear, 1)));
                }
                else if (soonestPerishHours > hoursPerday) {
                    dsc.AppendLine(", " + Lang.Get("foodshelves:will perish in {0} days", Math.Round(soonestPerishHours / hoursPerday, 1)));
                }
                else {
                    dsc.AppendLine(", " + Lang.Get("foodshelves:will perish in {0} hours", Math.Round(soonestPerishHours, 1)));
                }
            }
        }
        else {
            dsc.AppendLine(Lang.Get("foodshelves:No item will perish soon."));
        }
    }
}
