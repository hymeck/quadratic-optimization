namespace Demo
{
    public class Printer
    {
        public static void PrintSolution(QuadraticResult result)
        {
            var strRes = $"[{string.Join("; ", result)}]";
            System.Console.WriteLine(strRes);
        }
    }
}
