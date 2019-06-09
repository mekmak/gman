using System;
using System.Collections.Generic;
using System.Text;

namespace Mekmak.Gman.UI.Iron
{
    public class ConsoleCommand
    {
        private readonly int _index;
        private readonly string[] _tokens;

        private ConsoleCommand(string[] tokens, int index)
        {
            _tokens = tokens ?? new string[0];
            _index = index < 0 ? 0 : index;
        }

        public static ConsoleCommand New(string input)
        {
            return new ConsoleCommand(input.Split(' '), 0);
        }

        public string Current => _index >= _tokens.Length ? string.Empty : _tokens[_index];

        public ConsoleCommand Advance()
        {
            return new ConsoleCommand(_tokens, _index + 1);
        }

        public override string ToString()
        {
            return Current;
        }
    }
}
