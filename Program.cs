// See https://aka.ms/new-console-template for more information


using System.Text;

int incParamNum = 0;

string fileName = "";

string versionStr = null;

bool isVB = false;

for (int i = 0; i < args.Length; i++)
{
    if (args[i].StartsWith("-inc:"))
    {
        string s = args[i].Substring("-inc:".Length);
        incParamNum = int.Parse(s);
    }
    else if (args[i].StartsWith("-set:"))
    {
        versionStr = args[i].Substring("-set:".Length);
    }
    else
        fileName = args[i];
}

if (Path.GetExtension(fileName).ToLower() == ".vb")
    isVB = true;

if (fileName == "")
{
    System.Console.WriteLine("Usage: AssemblyInfoUtil <path to AssemblyInfo.cs or AssemblyInfo.vb file> [options]");
    System.Console.WriteLine("Options: ");
    System.Console.WriteLine("  -set:<new version number> - set new version number (in NN.NN.NN.NN format)");
    System.Console.WriteLine("  -inc:<parameter index>  - increases the parameter with specified index (can be from 1 to 4)");

    return;
}

if (!File.Exists(fileName))
{
    System.Console.WriteLine("Error: Can not find file \"" + fileName + "\"");

    return;
}

System.Console.Write("Processing \"" + fileName + "\"...");
StreamReader reader = new(fileName);
StreamWriter writer = new(fileName + ".out");
string line;

while ((line = reader.ReadLine()) != null)
{
    line = ProcessLine(line);
    writer.WriteLine(line);
}

reader.Close();
writer.Close();

File.Delete(fileName);
File.Move(fileName + ".out", fileName);
System.Console.WriteLine("Done!");



string ProcessLine(string ln)
{
    if (isVB)
    {
        ln = ProcessLinePart(ln, "<Assembly: AssemblyVersion(\"");
        ln = ProcessLinePart(ln, "<Assembly: AssemblyFileVersion(\"");
    }
    else
    {
        ln = ProcessLinePart(ln, "[assembly: AssemblyVersion(\"");
        ln = ProcessLinePart(ln, "[assembly: AssemblyFileVersion(\"");
    }

    return ln;
}

string ProcessLinePart(string ln, string part)
{
    int spos = ln.IndexOf(part, StringComparison.Ordinal);

    if (spos >= 0)
    {
        spos += part.Length;
        int epos = ln.IndexOf('"', spos);
        string oldVersion = ln.Substring(spos, epos - spos);
        string newVersion = "";
        bool performChange = false;

        if (incParamNum > 0)
        {
            string[] nums = oldVersion.Split('.');

            if (nums.Length >= incParamNum && nums[incParamNum - 1] != "*")
            {
                long val = long.Parse(nums[incParamNum - 1]);
                val++;
                nums[incParamNum - 1] = val.ToString();
                newVersion = nums[0];

                for (int i = 1; i < nums.Length; i++)
                {
                    newVersion += "." + nums[i];
                }

                performChange = true;
            }
        }
        else if (versionStr != null)
        {
            newVersion = versionStr;
            performChange = true;
        }

        if (performChange)
        {
            StringBuilder str = new(ln);
            str.Remove(spos, epos - spos);
            str.Insert(spos, newVersion);
            ln = str.ToString();
        }
    }

    return ln;
}
