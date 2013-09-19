using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using LAIR.Extensions;
using LAIR.IO;

namespace PTL.ATT.GUI
{
    public class LogWriter : StandardOutWriter
    {
        private TextBoxBase _textBox;
        private bool _scrollTextBox;

        public LogWriter(TextBoxBase textBox, string path, bool writeTimestamp, params TextWriter[] otherOutputs)
            : base(path, writeTimestamp, otherOutputs)
        {
            _textBox = textBox;
            _scrollTextBox = true;
            _textBox.MouseWheel += new MouseEventHandler(textBox_MouseWheel);
        }

        public override void Clear()
        {
            base.Clear();

            lock (_textBox)
            {
                if (_textBox.InvokeRequired)
                    _textBox.BeginInvoke(new Action(_textBox.Clear));
                else
                    _textBox.Clear();
            }
        }

        private void textBox_MouseWheel(object sender, MouseEventArgs args)
        {
            _scrollTextBox = _textBox.TextLength == _textBox.GetCharIndexFromPosition(new System.Drawing.Point(_textBox.ClientRectangle.Right, _textBox.ClientRectangle.Bottom)) + 1;
        }

        protected override string Write(string value, bool newLine)
        {
            if (_textBox.InvokeRequired)
            {
                _textBox.BeginInvoke(new Func<string, bool, string>(Write), value, newLine);
                return null;
            }
            else
            {
                value = base.Write(value, newLine);

                lock (_textBox)
                {
                    _textBox.AppendText(value);
                    if (_scrollTextBox)
                        _textBox.ScrollToCaret();
                }

                return value;
            }
        }
    }
}
