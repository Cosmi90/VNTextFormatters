using System.Text;
using System.Text.RegularExpressions;

bool debug = true;
bool dryrun = false;

string argNumberOfCharacters = null;
string argPathToScripts = null;
string argListOfScripts = null;
string argListOfIgnoredAddresses = null;
string argPathToMessage = null;

var categoryAText = new SortedList<int, string>();
var categoryBText = new SortedList<int, string>();
var categoryCText = new SortedList<int, string>();

if (args.Length == 0) {
    InvalidArguments();
}

for (int i = 0; i < args.Length; i++) {
    switch (args[i]) {
        case "-c":
            if (argNumberOfCharacters != null) {
                InvalidArguments();
            }
            argNumberOfCharacters = args[i + 1];
            break;
        case "-p":
            if (argPathToScripts != null) {
                InvalidArguments();
            }
            argPathToScripts = args[i + 1].Replace(@"'", "");
            break;
        case "-s":
            if (argListOfScripts != null) {
                InvalidArguments();
            }
            argListOfScripts = args[i + 1];
            break;
        case "-i":
            argListOfIgnoredAddresses = args[i + 1];
            break;
        case "-m":
            if (argPathToMessage != null) {
                InvalidArguments();
            }
            argPathToMessage = args[i + 1].Replace(@"'", "");
            break;
        case "--dry-run":
            dryrun = true;
            break;
    }
}

if (argNumberOfCharacters == null || argPathToScripts == null || argListOfScripts == null || argPathToMessage == null) {
    InvalidArguments();
}

if (debug) {
    Console.WriteLine("-c is: " + argNumberOfCharacters);
    Console.WriteLine("-p is: " + argPathToScripts);
    Console.WriteLine("-s is: " + argListOfScripts);
}

#region text formatting
if (argNumberOfCharacters != null && argPathToScripts != null && argListOfScripts != null) {
    // delete message file at start
    File.Delete(argPathToMessage);

    string[] scriptFiles = argListOfScripts.Split(',');
    List<string> ignoredAddresses = argListOfIgnoredAddresses.Split(',').ToList();

    foreach (string s in scriptFiles) {
        string fullScriptPath;
        if (!File.Exists(argPathToScripts + "\\" + s)) {
            fullScriptPath = argPathToScripts + "\\" + s + ".txt";
        }
        else {
            fullScriptPath = argPathToScripts + "\\" + s;
        }

        if (File.Exists(fullScriptPath)) {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("Formatting file: " + fullScriptPath);
            Console.ResetColor();

            string[] textInFile = File.ReadAllLines(fullScriptPath, Encoding.UTF8);

            int c;
            int n;
            // test if -n and -c are indeed convertable to int
            try {
                c = Int16.Parse(argNumberOfCharacters);
            }
            catch {
                InvalidArguments();
            }
            c = Int16.Parse(argNumberOfCharacters);

            for (int i = 0; i < textInFile.Length; i++) {
                Match diamond = Regex.Match(textInFile[i], @"(^.)");
                Match textCategory = Regex.Match(textInFile[i], @"^.{1}(.{1})");
                Match textHash = Regex.Match(textInFile[i], @"^.{2}(.{8})");
                Match dialogueLine = Regex.Match(textInFile[i], @"(\◇|\◆).{9}(\◇|\◆)(.+)");
                Match dialogueLineAddress = Regex.Match(textInFile[i], @"((\◇|\◆).{9}(\◇|\◆)).+");
                if (dialogueLine.Success) {
                    string text;
                    int position = int.Parse(textHash.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);

                    if (dialogueLine.Groups[3].Value.Length > c) {
                        text = FormatText(dialogueLineAddress.Groups[1].Value, dialogueLine.Groups[3].Value, c, debug);
                    }
                    else {
                        text = textInFile[i];
                    }

                    if (diamond.Value == "◇") {
                        if (textCategory.Groups[1].Value == "A") {
                            categoryAText.Add(position, text);
                        }
                        else if (textCategory.Groups[1].Value == "B") {
                            categoryBText.Add(position, text);
                        }
                        else if (textCategory.Groups[1].Value == "C") {
                            categoryCText.Add(position, text);
                        }
                    }
                }
            }
        }
        else {
            FileNotFound();
        }
    }

    // commit changes by writting the lines to file
    if (!dryrun) {
        //File.AppendAllLines(argPathToMessage, textInFile);
        foreach (KeyValuePair<int, string> kvp in categoryAText) {
            using (StreamWriter sw = File.AppendText(argPathToMessage)) {
                sw.WriteLine(kvp.Value);
                sw.WriteLine(kvp.Value.Replace("◇", "◆"));
                sw.WriteLine("");
            }
        }
        foreach (KeyValuePair<int, string> kvp in categoryBText) {
            using (StreamWriter sw = File.AppendText(argPathToMessage)) {
                sw.WriteLine(kvp.Value);
                sw.WriteLine(kvp.Value.Replace("◇", "◆"));
                sw.WriteLine("");
            }
        }
        foreach (KeyValuePair<int, string> kvp in categoryCText) {
            using (StreamWriter sw = File.AppendText(argPathToMessage)) {
                // don't add new line to special characters
                if (ignoredAddresses.Contains(kvp.Key.ToString("X8"))) {
                    sw.WriteLine(kvp.Value);
                    sw.WriteLine(kvp.Value.Replace("◇", "◆"));
                    sw.WriteLine("");
                }
                else {
                    sw.WriteLine(kvp.Value + "\\n");
                    sw.WriteLine(kvp.Value.Replace("◇", "◆") + "\\n");
                    sw.WriteLine("");
                }
            }
        }
    }
}
#endregion

string FormatText(string dialogueLineId, string input, int charsPerLine, bool debug) {
    int currentIndexPosition = 0;
    while (input.Length > currentIndexPosition + charsPerLine) {
        currentIndexPosition += charsPerLine;
        while (input[currentIndexPosition] != ' ') {
            // 『』kagikakko handling
            if (input[currentIndexPosition] == '『') {
                //currentIndexPosition--;
                break;
            }
            //else if (input[currentIndexPosition] == '』') {
            //    currentIndexPosition++;
            //    break;
            //}
            else {
                currentIndexPosition--;
            }
        }
        input = input.Insert(currentIndexPosition, "\\n");
        currentIndexPosition = input.LastIndexOf("\\n") + 2;
        // removes white space at the beginning of new line
        if (input[currentIndexPosition] == ' ') {
            input = input.Remove(currentIndexPosition, 1);
        }
    }

    if (debug) {
        Console.WriteLine(dialogueLineId + input);
    }

    return dialogueLineId + input;
}

void FileNotFound() {
    Console.WriteLine("Specified path does not exist!\n");
    Environment.Exit(1);
}

void InvalidArguments() {
    Console.WriteLine("Missing or invalid arguments specified!\n");
    PrintHelp();
    Environment.Exit(1);
}

void PrintHelp() {
    Console.WriteLine("Atelier Kaguya visual novel game engine text formatter\n" +
        "This tool formats the characters of translated script files by replacing characters\n" +
        "of incomplete words that are at the right edge of the dialogue lines with \\n new line\n" +
        "character in order to better format the text\n" +
        "\n" +
        "To use this tool:\n" +
        "-c : number of characters per line\n" +
        "-p : path to Script files between single quotes\n" +
        "-s : comma separated list of script files that need formatting\n" +
        "-i : comma separated list of hex addresses that should be ignored, e.g. ending markers\n" +
        "-m : path to message.txt file for conversion to message.dat\n" +
        "--dry-run : [optional] will only output the changes and not modify the files" +
        "\n" +
        "Example usage: KaguyaTextFormatter.exe -c 50 -n 3 -p 'C:\\path\\to\\scripts' -s sc_a01,sc_a02,sc_a03" +
        "\n");
}