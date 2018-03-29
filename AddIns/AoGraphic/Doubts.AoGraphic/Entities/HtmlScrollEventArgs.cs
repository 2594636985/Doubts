using System;
using System.Drawing;

namespace Doubts.AoGraphic.Entities
{
    /// <summary>
    /// Raised when Html Renderer request scroll to specific location.<br/>
    /// This can occur on document anchor click.
    /// </summary>
    public sealed class HtmlScrollEventArgs : EventArgs
    {
        /// <summary>
        /// the location to scroll to
        /// </summary>
        private readonly Point _location;

        /// <summary>
        /// init.
        /// </summary>
        /// <param name="location">the location to scroll to</param>
        public HtmlScrollEventArgs(Point location)
        {
            _location = location;
        }

        /// <summary>
        /// the location to scroll to
        /// </summary>
        public Point Location
        {
            get { return _location; }
        }

        public override string ToString()
        {
            return string.Format("Location: {0}", _location);
        }
    }
}
