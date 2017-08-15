namespace Niffler.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Niffler.Model.KeyLevel YesterdayKeyLevels = Business.KeyLevels.GetYesterdaysKeyLevels("IC Markets", "UK100");
        }
    }
}
