using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pospa.MJPEG
{
    public class MultipartStreamReader : IDisposable
    {
        private const string ContentTypeRegex = @"Content-Type:\s(?<type>[^;]*);\sboundary=(?<boundary>.*)";
        private const string RegexBoundaryGroupKey = "boundary";
        private const string ContentLengthRegex = @"Content-Length:\s(?<length>\d*)";
        private const string RegexLengthGroupKey = "length";
        private readonly StreamReader _streamReader;
        private readonly string _boundaryDelimiter;
        public bool IsEndOfStream => _streamReader.EndOfStream;

        public MultipartStreamReader(Stream stream)
        {
            Regex regex = new Regex(ContentTypeRegex);
            _streamReader = new StreamReader(stream);
            string line;
            do
            {
                line = _streamReader.ReadLine();
                if (regex.IsMatch(line))
                {
                    Match match = regex.Match(line);
                    _boundaryDelimiter = match.Groups[RegexBoundaryGroupKey].Value;
                }
            } while (string.IsNullOrEmpty(line));
        }

        public void Dispose()
        {
            _streamReader.Dispose();
        }

        public async Task GetNextPartAsync()
        {
            Regex regex = new Regex(ContentLengthRegex);
            int contentLength = 0;
            string line;
            do
            {
                line = await _streamReader.ReadLineAsync();
            } while (line.Equals(_boundaryDelimiter));
            do
            {
                line = await _streamReader.ReadLineAsync();
                if (regex.IsMatch(line))
                {
                    Match match = regex.Match(line);
                    contentLength = int.Parse(match.Groups[RegexLengthGroupKey].Value);
                }
            } while (string.IsNullOrEmpty(line));
            char[] buffer = new char[contentLength];
            _streamReader.ReadBlockAsync(buffer, 0, contentLength);
        }
    }
}