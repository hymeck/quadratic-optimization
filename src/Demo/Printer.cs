namespace Demo
{
    public class Printer
    {
        public static void PrintSolution(QuadraticResult result)
        {
            
            var strRes = result.Solution.Length == 0
                ? "[]"
                : $"[{string.Join("; ", result.Solution)}]";
            System.Console.WriteLine(strRes);
        }
    }
}
