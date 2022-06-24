using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Ra.Subtitles
{
    public class SubtitleParser
    {
     
        public IEnumerable<SubtitleItem> Parse(string path)
        {
            return Parse(File.OpenRead(path));
        }
     
        public IEnumerable<SubtitleItem> Parse(Stream stream)
        {
            return Parse(new StreamReader(stream));
        }
     
        enum SubtitleParserStep
        {
            None,
            Id,
            Time,
            Text
        }
     
        public IEnumerable<SubtitleItem> Parse(TextReader reader)
        {
            using(reader)
            {
                SubtitleItem current = null;
                string line;
                SubtitleParserStep step = SubtitleParserStep.None;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line))
                    {
                        if (current != null)
                            yield return current;
                        current = null;
                        step = SubtitleParserStep.None;
                        continue;
                    }
     
                    if (current == null)
                        current = new SubtitleItem();
     
                    if (step != SubtitleParserStep.Text)
                        step++;
     
                    switch (step)
                    {
                        case SubtitleParserStep.Id:
                            if (!ParseId(line, current))
                            {
                                // id is missing, skip to Time
                                step++;
                                goto case SubtitleParserStep.Time;
                            }
                            break;
                        case SubtitleParserStep.Time:
                            if (!ParseTime(line, current))
                            {
                                throw new FormatException("Le format de temps est incorrect");
                            }
                            break;
                        case SubtitleParserStep.Text:
                            if (!string.IsNullOrEmpty(current.Text))
                                current.Text += Environment.NewLine;
                            current.Text += line;
                            break;
                    }
                }
     
                if (current != null)
                    yield return current;
            }
        }
     
        bool ParseId(string line, SubtitleItem current)
        {
            int id;
            if (int.TryParse(line, out id))
            {
                current.Id = id;
                return true;
            }
            return false;
        }
     
        static readonly CultureInfo _subtitleTimeCulture = CultureInfo.GetCultureInfo("fr-FR");
        static readonly Regex _subtitleTimeRegex = new Regex("(?<start>[0-9:,]+) --> (?<end>[0-9:,]+)", RegexOptions.Compiled);
     
        bool ParseTime(string line, SubtitleItem current)
        {
            Match m = _subtitleTimeRegex.Match(line);
            if (m.Success)
            {
                TimeSpan start;
                TimeSpan end;
     
                if (!TimeSpan.TryParse(m.Groups["start"].Value, _subtitleTimeCulture, out start))
                    return false;
     
                if (!TimeSpan.TryParse(m.Groups["end"].Value, _subtitleTimeCulture, out end))
                    return false;
     
                current.StartOffset = start;
                current.EndOffset = end;
                return true;
            }
            return false;
        }
    }
     
    public class SubtitleItem
    {
        public int Id { get; set; }
        public TimeSpan StartOffset { get; set; }
        public TimeSpan EndOffset { get; set; }
        public string Text { get; set; }
    }
}