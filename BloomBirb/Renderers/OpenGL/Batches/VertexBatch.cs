
using BloomBirb.Graphics.Vertices;
using BloomBirb.Renderers.OpenGL.Buffers;

namespace BloomBirb.Renderers.OpenGL.Batches
{
    public abstract class VertexBatch<T> : IDisposable where T : unmanaged, IEquatable<T>, IVertex
    {
        private VertexBuffer<T>[] buffers;

        private int bufferCount;

        private int currentBufferIndex;

        protected readonly OpenGLRenderer Renderer;

        public VertexBatch(OpenGLRenderer renderer, int numberOfBuffers, int bufferSize)
        {
            Renderer = renderer;
            bufferCount = numberOfBuffers;
            buffers = new VertexBuffer<T>[numberOfBuffers];

            for (int i = 0; i < bufferCount; i++)
                buffers[i] = CreateVertexBuffer(bufferSize);
        }

        public void Initialize()
        {
            foreach (VertexBuffer<T> buffer in buffers)
                buffer.Initialize();
        }

        public void AddVertex(T vertex)
        {
            VertexBuffer<T> currentBuffer = buffers[currentBufferIndex];

            currentBuffer.AddVertex(ref vertex);
            if (currentBuffer.IsFull)
            {
                currentBuffer.DrawBuffer();
                currentBufferIndex = (currentBufferIndex + 1) % bufferCount;
            }
        }

        public void FlushBatch()
        {
            if (buffers[currentBufferIndex].Count > 0)
                buffers[currentBufferIndex].DrawBuffer();

            currentBufferIndex = 0;
        }

        protected abstract VertexBuffer<T> CreateVertexBuffer(int size);

        public void Dispose()
        {
            foreach (VertexBuffer<T> buffer in buffers)
                buffer.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
