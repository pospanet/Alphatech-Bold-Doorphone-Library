using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.BAL
{
    internal class BoldEventsStreamReader : IDisposable
    {
        private const string SettingSectionHeader = "[setting]";
        private const string EventSectionHeader = "[evstat]";

        private readonly StreamReader _streamReader;
        private string _lastDataLine;

        public BoldEventsStreamReader(Stream stream)
        {
            _streamReader = new StreamReader(stream);
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
            if (_streamReader.EndOfStream)
            {
                return null;
            }
            do
            {
                _lastDataLine = _streamReader.ReadLine().Trim();
            } while (!_lastDataLine.Equals(sectionHeader) || !_streamReader.EndOfStream);
            if (_streamReader.EndOfStream)
            {
                return null;
            }
            Dictionary<string, string> sectionData = new Dictionary<string, string>();
            do
            {
                string[] pair = _lastDataLine.Split('=');
                sectionData.Add(pair[0].Trim(), pair[1].Trim('"').Trim());
                _lastDataLine = _streamReader.ReadLine().Trim();
            } while (!string.IsNullOrEmpty(_lastDataLine));
            return sectionData;
        }

        public void Dispose()
        {
            _streamReader.Dispose();
        }
    }
}