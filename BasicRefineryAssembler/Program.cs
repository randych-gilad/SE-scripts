using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.Screens;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
  partial class Program : MyGridProgram
  {
    // Global declaration for constructor at compile time
    static bool debug = false;
    IMyInventory refineryOutput;
    IMyInventory assemblerInput;
    IMyInventory assemblerOutput;
    IMyInventory assemblerContainer;
    public Program()
    {
      // UpdateFrequency.None -> on demand
      // UpdateFrequency.Update10 -> 10 ticks (<1 seconds)
      // UpdateFrequency.Update100 -> 100 ticks (1.7 seconds)
      Runtime.UpdateFrequency = UpdateFrequency.Update100;

      refineryOutput = GridTerminalSystem.GetBlockWithName("Refinery").GetInventory(1);
      assemblerInput = GridTerminalSystem.GetBlockWithName("Assembler").GetInventory(0);
      assemblerOutput = GridTerminalSystem.GetBlockWithName("Assembler").GetInventory(1);
      assemblerContainer = GridTerminalSystem.GetBlockWithName("Assembler Output").GetInventory(0);
    }

    public void Main()
    {
      IMyTextSurface screen = Me.GetSurface(0);
      screen.FontSize = 0.5F;

      if (refineryOutput == null || assemblerInput == null || assemblerContainer == null)
      {
        throw new Exception("Could not access one of inventories");
      }

      if (!IsStart(refineryOutput.ItemCount, screen) && !IsStart(assemblerOutput.ItemCount, screen))
      {
        return;
      }
      TransferItems(refineryOutput, assemblerInput, screen);
      TransferItems(assemblerOutput, assemblerContainer, screen);
    }

    public bool IsEnoughSpace(IMyInventory src, IMyInventory dst, IMyTextSurface screen)
    {
      int itemCount = src.ItemCount;
      MyInventoryItem item = src.GetItemAt(0).Value;
      int decimalAmount = (int)item.Amount;
      MyFixedPoint itemVolume = (MyFixedPoint)src.GetItemAt(0).Value.Type.GetItemInfo().Volume;

      MyFixedPoint availableSpace = dst.MaxVolume - dst.CurrentVolume;

      if (availableSpace < itemVolume)
      {
        screen.WriteText($"ERROR: Not enough space to transfer {decimalAmount} {item.Type.SubtypeId}");
        if (debug)
        {
          screen.WriteText($"max volume: {dst.MaxVolume}, current space: {dst.CurrentVolume}, amount: {decimalAmount}, available space: {availableSpace}");
          screen.WriteText($"ERROR: Need {(int)(item.Amount - (int)availableSpace)} more space");
        }
        return false;
      }
      return true;
    }

    public void TransferItems(IMyInventory src, IMyInventory dst, IMyTextSurface screen)
    {
      int itemCount = src.ItemCount;

      while (itemCount > 0)
      {
        MyInventoryItem item = src.GetItemAt(0).Value;

        if (!src.CanTransferItemTo(dst, item.Type))
        {
          Echo($"ERROR: no connection between refinery and assembler");
          return;
        }

        if (debug)
        {
          screen.WriteText($"Type id: {item.Type.TypeId}, Subtype id: {item.Type.SubtypeId}");
        }

        if (!IsEnoughSpace(src, dst, screen)) { return; }

        int decimalAmount = (int)item.Amount;
        {
          bool succ = dst.TransferItemFrom(src, 0, null, true, null);
          if (succ)
          {
            screen.WriteText($"Transferred {decimalAmount} {item.Type.SubtypeId} from source to destination");
            itemCount--;
          }
          else
          {
            screen.WriteText($"Failed to transfer {decimalAmount} {item.Type.SubtypeId}");
          }
        }
      }

      if (itemCount == 0)
      {
        Echo("Transfer complete");
      }
    }

    public bool IsStart(int itemCount, IMyTextSurface screen)
    {
      if (itemCount == 0)
      {
        screen.WriteText("Nothing to transfer");
        return false;
      }
      else
      {
        screen.WriteText("Started transfer process");
        return true;
      }
    }
  }
}
