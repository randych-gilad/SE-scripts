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

}

public void TransferItems(IMyInventory src, IMyInventory dst)
{
  int itemCount = src.ItemCount;

  while (itemCount > 0)
  {
    MyInventoryItem item = src.GetItemAt(0).Value;
    int decimalAmount = (int) item.Amount;

    if (debug)
      {
      Echo($"Type id: {item.Type.TypeId}, Subtype id: {item.Type.SubtypeId}");
      }

    if (!CanAcceptItem(src, dst)) { return; }

    // if (item.Type.SubtypeId == "Stone")
    // {
    //     if (!CanAcceptItem(src, dstGravel)) { return; }
    //     bool succ = dstGravel.TransferItemFrom(src, 0, null, true, null);
    //     if (succ)
    //     {
    //         Echo($"Transferred {decimalAmount} Gravel to cargo container");
    //         itemCount--;
    //     }
    //     else
    //     {
    //         Echo($"ERROR: Failed to transfer {decimalAmount} Gravel");
    //         Echo($"ERROR: Check conveyor from refinery to cargo container");
    //         Echo($"INFO: There are no checks for gravel container free space");
    //         return;
    //     }
    // }
    // else

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