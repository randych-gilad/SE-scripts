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