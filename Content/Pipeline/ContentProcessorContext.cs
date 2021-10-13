using System;
using System.Threading;

namespace engenious.Content.Pipeline
{
    /// <summary>
    ///     Content context for a build process in the processing stage using <see cref="IContentProcessor"/>.
    /// </summary>
    public class ContentProcessorContext : ContentContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentProcessorContext"/> class.
        /// </summary>
        /// <param name="syncContext">The synchronization context used for UI interaction.</param>
        /// <param name="game">
        ///     The engenious Game used for graphical interactions.
        ///     <remarks>
        ///         Is <see cref="object"/> because project needs to be .netstandard(for source generator)
        ///         and can therefore not reference the engenious base library.
        ///     </remarks>
        /// </param>
        /// <param name="buildId">The build id for the current building process.</param>
        /// <param name="createdContentCode">The created content code to write all generated code to.</param>
        /// <param name="contentDirectory">The directory the content is located in.</param>
        /// <param name="workingDirectory">The working directory for the building process.</param>
        public ContentProcessorContext(SynchronizationContext syncContext, object game, Guid buildId,
            CreatedContentCode createdContentCode, string contentDirectory, string workingDirectory = "")
            : base(buildId, createdContentCode, contentDirectory, workingDirectory)
        {
            SyncContext = syncContext;


            Game = game;
            // GraphicsDevice.SwitchUiThread();
        }

        /// <summary>
        ///     Gets the synchronization context used for UI interaction.
        /// </summary>
        public SynchronizationContext SyncContext { get; }

        /// <summary>
        ///     Gets the engenious Game used for graphical interactions.
        /// </summary>
        /// <remarks>
        ///     Is <see cref="object"/> because project needs to be .netstandard(for source generator)
        ///     and can therefore not reference the engenious base library.
        /// </remarks>
        public object Game { get; }
        // public GraphicsDevice GraphicsDevice => Game.GraphicsDevice;

        /// <inheritdoc />
        public override void Dispose()
        {
            // GraphicsDevice.Context.MakeNoneCurrent();
            //Window.Dispose();
        }
    }
}