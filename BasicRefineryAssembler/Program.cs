using Sandbox.Game.EntityComponents;
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
    public Program()
    {
      // UpdateFrequency.None -> on demand
      // UpdateFrequency.Update10 -> 10 ticks (<1 seconds)
      // UpdateFrequency.Update100 -> 100 ticks (1.7 seconds)
      Runtime.UpdateFrequency = UpdateFrequency.Update100;
    }

    bool debug = false;

    public void Main()
    {
      IMyInventory refineryOutput = GridTerminalSystem.GetBlockWithName("Refinery").GetInventory(1);
      IMyInventory assemblerInput = GridTerminalSystem.GetBlockWithName("Assembler").GetInventory(0);
      IMyInventory assemblerOutput = GridTerminalSystem.GetBlockWithName("Assembler").GetInventory(1);
      IMyInventory assemblerContainer = GridTerminalSystem.GetBlockWithName("Assembler Output").GetInventory(0);

      if (refineryOutput == null || assemblerInput == null || assemblerContainer == null)
      {
        throw new Exception("Could not access one inventories");
      }

      if (!IsStart(refineryOutput.ItemCount) && !IsStart(assemblerOutput.ItemCount))
      {
        return;
      }
      TransferItems(refineryOutput, assemblerInput);
      TransferItems(assemblerOutput, assemblerContainer);
    }

    public bool IsEnoughSpace(IMyInventory src, IMyInventory dst)
    {
      int itemCount = src.ItemCount;
      MyInventoryItem item = src.GetItemAt(0).Value;
      int decimalAmount = (int)item.Amount;
      MyFixedPoint itemVolume = (MyFixedPoint)src.GetItemAt(0).Value.Type.GetItemInfo().Volume;

      MyFixedPoint availableSpace = dst.MaxVolume - dst.CurrentVolume;

      if (availableSpace < itemVolume)
      {
        Echo($"ERROR: Not enough space to transfer {decimalAmount} {item.Type.SubtypeId}");
        if (debug)
        {
          Echo($"max volume: {dst.MaxVolume}, current space: {dst.CurrentVolume}, amount: {decimalAmount}, available space: {availableSpace}");
          Echo($"ERROR: Need {(int)(item.Amount - (int)availableSpace)} more space");
        }
        return false;
      }
      return true;
    }

    public void TransferItems(IMyInventory src, IMyInventory dst)
    {
      int itemCount = src.ItemCount;

      while (itemCount > 0)
      {
        MyInventoryItem item = src.GetItemAt(0).Value;

        if (!src.CanTransferItemTo(dst, item.Type))
        {
          Echo("ERROR: no connection between refinery and assembler");
          return;
        }

        if (debug)
        {
          Echo($"Type id: {item.Type.TypeId}, Subtype id: {item.Type.SubtypeId}");
        }

        if (!IsEnoughSpace(src, dst)) { return; }

        int decimalAmount = (int)item.Amount;
        {
          bool succ = dst.TransferItemFrom(src, 0, null, true, null);
          if (succ)
          {
            Echo($"Transferred {decimalAmount} {item.Type.SubtypeId} from source to destination");
            itemCount--;
          }
          else
          {
            Echo($"Failed to transfer {decimalAmount} {item.Type.SubtypeId}");
          }
        }
      }

      if (itemCount == 0)
      {
        Echo("Transfer complete");
      }
    }

    public bool IsStart(int itemCount)
    {
      if (itemCount == 0)
      {
        Echo("Nothing to transfer");
        return false;
      }
      else
      {
        Echo("Started transfer process");
        return true;
      }
    }
  }
}
