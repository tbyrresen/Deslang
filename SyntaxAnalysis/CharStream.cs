using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Deslang.SyntaxAnalysis
{
    public class CharStream
    {
        private readonly StreamReader _streamReader;
        private char _nextChar;
        private bool _EOF;

        public CharStream(FileStream fileStream)
        {
            _streamReader = new StreamReader(fileStream);
            _nextChar = '\0';
            _EOF = false;
            Read();
        }

        public char Peek() => _nextChar;

        public bool EOF() => _EOF; 

        public char Read()
        {
            char answer = _nextChar;
            try
            {
                int nextStreamChar = _streamReader.Read();
                if (nextStreamChar == -1)
                {
                    _EOF = true;
                    _nextChar = '\0';
                }
                else
                {
                    _nextChar = (char)nextStreamChar;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error during fileread: " + e.Message);
                _EOF = true;
                return '\0';
            }
            return answer;
        }
    }
}
