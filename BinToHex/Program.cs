

using IntelHex;
using System.Text;

namespace BinToHex;

internal class Program
{
    static void Main(string[] args)
    {
        if(args.Count() >= 4)
        {
            if (args[0] == "-s" && args[2] == "-o")
            {
                Console.WriteLine("Bin to hex converting started...");

                IntelHexGenerator intelHexGenerator = new IntelHexGenerator();
                var bin = File.ReadAllBytes(args[1]);
                Console.WriteLine($"{args[1]} file readed");
                var hex = intelHexGenerator.GetIntelHex(0, bin);
                Console.WriteLine($"Converting DONE");
                File.WriteAllBytes(args[3], Encoding.UTF8.GetBytes(hex));
                Console.WriteLine($"{args[3]} file saved");
            }
            else
            {
                Console.WriteLine("Incorrect command");
            }
        }
        else
        {
            Console.WriteLine("Incorrect count of arguments");
        }
    }
}