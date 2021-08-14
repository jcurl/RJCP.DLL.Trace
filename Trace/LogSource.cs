namespace RJCP.Diagnostics.Trace
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// A <see cref="System.Diagnostics.TraceSource"/> wrapper for better performance tracing.
    /// </summary>
    /// <remarks>
    /// Use the <see cref="LogSource"/> class instead of a <see cref="System.Diagnostics.TraceSource"/> class to
    /// initialize logging, so that tests can be done with the <see cref="ShouldTrace(TraceEventType)"/> method.
    /// <para>
    /// The goal is to allow classes to test if tracing is enabled or not, before using the <see cref="TraceSource"/>.
    /// This can help prevention of unnecessary objects on the heap by tracing only when tracing is needed.
    /// </para>
    /// <code language="csharp">
    ///<![CDATA[
    ///private static LogSource log = new LogSource("MyTrace");
    ///if (log.ShouldTrace(TraceEventType.Information))
    ///log.TraceSource.TraceEvent(TraceEventType.Information, id, "Log Message: {0}", errorCode);
    ///]]>
    /// </code>
    /// <para>
    /// This allows first a check if tracing is allowed or not, by doing a bitwise comparison, and only then if tracing
    /// is enabled, will the TraceEvent function be called thus allocating the <c>object[]</c> for params, then boxing
    /// the integer value <c>errorCode</c> into the <c>object[]</c> array.
    /// </para>
    /// </remarks>
    public sealed class LogSource : IDisposable
    {
        private readonly string m_Name;
        private readonly long m_TraceLevels;
        private TraceSource m_TraceSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogSource"/> class with the
        /// <see cref="System.Diagnostics.TraceSource"/> of the name provided.
        /// </summary>
        /// <param name="name">The name of the trace source.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> may not be <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> may not be empty.</exception>
        public LogSource(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name may not be empty", nameof(name));
            m_Name = name;

            m_TraceSource = new TraceSource(m_Name);
            m_TraceLevels = GetTraceLevels(m_TraceSource);
        }

        /// <summary>
        /// Gets the trace source.
        /// </summary>
        /// <value>The trace source object.</value>
        /// <exception cref="System.ObjectDisposedException"/>
        public TraceSource TraceSource
        {
            get
            {
                if (m_TraceSource != null) return m_TraceSource;
                throw new ObjectDisposedException(m_Name);
            }
        }

        private static long GetTraceLevels(TraceSource source)
        {
            long levels = 0;
            if (source.Switch.ShouldTrace(TraceEventType.Critical)) levels |= (long)TraceEventType.Critical;
            if (source.Switch.ShouldTrace(TraceEventType.Error)) levels |= (long)TraceEventType.Error;
            if (source.Switch.ShouldTrace(TraceEventType.Warning)) levels |= (long)TraceEventType.Warning;
            if (source.Switch.ShouldTrace(TraceEventType.Information)) levels |= (long)TraceEventType.Information;
            if (source.Switch.ShouldTrace(TraceEventType.Verbose)) levels |= (long)TraceEventType.Verbose;
#if NETFRAMEWORK
            if (source.Switch.ShouldTrace(TraceEventType.Start)) levels |= (long)TraceEventType.Start;
            if (source.Switch.ShouldTrace(TraceEventType.Stop)) levels |= (long)TraceEventType.Stop;
            if (source.Switch.ShouldTrace(TraceEventType.Suspend)) levels |= (long)TraceEventType.Suspend;
            if (source.Switch.ShouldTrace(TraceEventType.Resume)) levels |= (long)TraceEventType.Resume;
            if (source.Switch.ShouldTrace(TraceEventType.Transfer)) levels |= (long)TraceEventType.Transfer;
#endif

            return levels;
        }

        /// <summary>
        /// Tests if tracing should be done for the log level provided.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns><see langword="true"/> if tracing is enabled; otherwise <see langword="false"/>.</returns>
        /// <remarks>Note, changing the trace source log level on the fly will not result in an update here.</remarks>
        public bool ShouldTrace(TraceEventType eventType)
        {
            return ((m_TraceLevels & (long)eventType) != 0);
        }

        /// <summary>
        /// Writes a trace event message to the trace listeners in the <see cref="TraceSource.Listeners"/> collection.
        /// </summary>
        /// <param name="eventType">
        /// One of the enumeration values that specifies the event type of the trace data.
        /// </param>
        /// <param name="message">The trace message to write.</param>
        public void TraceEvent(TraceEventType eventType, string message)
        {
            TraceSource.TraceEvent(eventType, 0, message);
        }

        /// <summary>
        /// Writes a trace event message to the trace listeners in the <see cref="TraceSource.Listeners"/> collection.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="format">
        /// A composite format string that contains text intermixed with zero or more format items, which correspond to
        /// objects in the <paramref name="args"/> array.
        /// </param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        public void TraceEvent(TraceEventType eventType, string format, params object[] args)
        {
            TraceSource.TraceEvent(eventType, 0, format, args);
        }

        /// <summary>
        /// Traces the exception with a level of detail based on the trace level.
        /// </summary>
        /// <param name="e">The exception to trace.</param>
        /// <param name="function">The function where the exception occurred.</param>
        /// <param name="message">The message to trace in addition.</param>
        public void TraceException(Exception e, string function, string message)
        {
            string logMessage;
            if (ShouldTrace(TraceEventType.Information)) {
                logMessage = string.Format("{0}: {1}{3}{2}", function, message, e, Environment.NewLine);
                TraceSource.TraceEvent(TraceEventType.Error, 0, logMessage);
            } else if (ShouldTrace(TraceEventType.Error)) {
                logMessage = string.Format("{0}: {1}{3}{2}", function, message, e.Message, Environment.NewLine);
                TraceSource.TraceEvent(TraceEventType.Error, 0, logMessage);
            }
        }

        /// <summary>
        /// Traces the exception with a level of detail based on the trace level.
        /// </summary>
        /// <param name="e">The exception to trace.</param>
        /// <param name="function">The function where the exception occurred.</param>
        /// <param name="format">The message to trace in addition.</param>
        /// <param name="args">The arguments used for formatting within <paramref name="format"/>.</param>
        public void TraceException(Exception e, string function, string format, params object[] args)
        {
            TraceException(e, function, string.Format(format, args));
        }

        /// <summary>
        /// Traces the exception with a level of detail based on the trace level.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="e">The exception to trace.</param>
        /// <param name="function">The function where the exception occurred.</param>
        /// <param name="message">The message to trace in addition.</param>
        public void TraceException(string name, Exception e, string function, string message)
        {
            string logMessage;
            if (ShouldTrace(TraceEventType.Information)) {
                logMessage = string.Format("{0}: {1}: {2}{4}{3}", name, function, message, e, Environment.NewLine);
                TraceSource.TraceEvent(TraceEventType.Error, 0, logMessage);
            } else if (ShouldTrace(TraceEventType.Error)) {
                logMessage = string.Format("{0}: {1}: {2}{4}{3}", name, function, message, e.Message, Environment.NewLine);
                TraceSource.TraceEvent(TraceEventType.Error, 0, logMessage);
            }
        }

        /// <summary>
        /// Traces the exception with a level of detail based on the trace level.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="e">The exception to trace.</param>
        /// <param name="function">The function where the exception occurred.</param>
        /// <param name="format">The message to trace in addition.</param>
        /// <param name="args">The arguments used for formatting within <paramref name="format"/>.</param>
        public void TraceException(string name, Exception e, string function, string format, params object[] args)
        {
            TraceException(name, e, function, string.Format(format, args));
        }

        private bool m_IsDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting managed and unmanaged
        /// resources.
        /// </summary>
        public void Dispose()
        {
            if (m_IsDisposed) return;

            m_TraceSource.Flush();
            m_TraceSource.Close();
            m_TraceSource = null;
            m_IsDisposed = true;
        }
    }
}
