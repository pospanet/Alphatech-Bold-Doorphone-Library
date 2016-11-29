using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.BAL
{
    internal class BoldEventsStreamReader : IDisposable
    {
        private const string SettingSectionHeader = "[setting]";
        private const string EventSectionHeader = "[evstat]";

        private readonly StringReader _streamReader;
        private string _lastDataLine;

        public BoldEventsStreamReader(string text)
        {
            _streamReader = new StringReader(text);
        }

        public Dictionary<string, string> GetSetting()
        {
            return GetSection(SettingSectionHeader);
        }

        public Dictionary<string, string> GetNextEvent()
        {
            return GetSection(EventSectionHeader);
        }

        private Dictionary<string, string> GetSection(string sectionHeader)
        {
            if (_streamReader.Peek().Equals(-1))
            {
                return null;
            }
            _lastDataLine = _streamReader.ReadLine().Trim();
            while (!_lastDataLine.Equals(sectionHeader) && !_streamReader.Peek().Equals(-1))
            {
                _lastDataLine = _streamReader.ReadLine().Trim();
            } 
            if (_streamReader.Peek().Equals(-1))
            {
                return null;
            }
            Dictionary<string, string> sectionData = new Dictionary<string, string>();
            _lastDataLine = _streamReader.ReadLine().Trim();
            while (!string.IsNullOrEmpty(_lastDataLine))
            {
                string[] pair = _lastDataLine.Split('=');
                sectionData.Add(pair[0].Trim(), pair[1].Trim('"').Trim());
                _lastDataLine = _streamReader.ReadLine().Trim();
            }
            return sectionData;
        }

        public void Dispose()
        {
            _streamReader.Dispose();
        }
    }
}