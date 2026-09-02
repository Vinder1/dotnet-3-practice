// task1
var word = Console.ReadLine();
if (string.IsNullOrWhiteSpace(word))
    word = "Привет";
var reversedWord = word.Reverse().ToArray();
Console.WriteLine(reversedWord);

// task2
var line = Console.ReadLine();
if (line == null)
    return;
var words = line.Split();
Console.WriteLine(string.Join(" ",
    words.Select(w => new string(w.Reverse().ToArray()))
));