﻿namespace RJCP.Diagnostics.Trace.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    internal sealed class LineSplitter : IEnumerable<string>
    {
        private readonly StringBuilder m_Line = new();
        private List<string> m_Lines = new();

        public bool IsCached { get { return m_Line.Length > 0; } }

        public void Append(string line)
        {
            AppendInternal(line);
        }

        public void AppendLine(string line)
        {
            AppendInternal(line);
            m_Lines.Add(m_Line.ToString());
            m_Line.Clear();
        }

        public void NewLine()
        {
            m_Lines.Add(m_Line.ToString());
            m_Line.Clear();
        }

        public IEnumerator<string> GetEnumerator()
        {
            List<string> result = m_Lines;
            m_Lines = new List<string>();
            return result.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void AppendInternal(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;

            string[] lines = line.Split('\n');
            m_Line.Append(lines[0]);

            // There was no newline
            if (lines.Length == 1) return;

            m_Lines.Add(m_Line.ToString());
            m_Line.Clear();
            if (lines.Length > 2) {
                for (int i = 1; i < lines.Length - 1; i++) {
                    m_Lines.Add(lines[i]);
                }
            }
            m_Line.Append(lines[^1]);
        }
    }
}
