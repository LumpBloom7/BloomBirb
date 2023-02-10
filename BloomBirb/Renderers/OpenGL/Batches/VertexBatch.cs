using BloomBirb.Graphics.Vertices;
using BloomBirb.Renderers.OpenGL.Buffers;

namespace BloomBirb.Renderers.OpenGL.Batches
{
    public abstract class VertexBatch<T> : IDisposable where T : unmanaged, IEquatable<T>, IVertex
    {
        private VertexBuffer<T>[] buffers;

        private readonly int maxBuffers;
        private int availableBuffers;

        private int currentBufferIndex;

        private readonly int bufferSize;

        protected readonly OpenGLRenderer Renderer;

        private readonly Stack<T> translucentVertices = new Stack<T>();

        public VertexBatch(OpenGLRenderer renderer, int bufferSize, int maxNumberOfBuffers = 100)
        {
            Renderer = renderer;
            this.bufferSize = bufferSize;
            maxBuffers = maxNumberOfBuffers;
            buffers = new VertexBuffer<T>[maxNumberOfBuffers];
        }

        public void Initialize()
        {
            buffers[0] = CreateVertexBuffer(bufferSize);
            buffers[0].Initialize();
            availableBuffers = 1;
        }

        public void AddVertex(T vertex, bool isOpaque = true)
        {
            // Defer translucent objects until after opaque ones are drawn
            if (!isOpaque)
            {
                translucentVertices.Push(vertex);
                return;
            }

            VertexBuffer<T> currentBuffer = buffers[currentBufferIndex];

            currentBuffer.AddVertex(ref vertex);

            if (currentBuffer.IsFull)
            {
                currentBuffer.DrawBuffer();
                currentBufferIndex = (currentBufferIndex + 1) % maxBuffers;

                if (currentBufferIndex == availableBuffers)
                {
                    if (currentBufferIndex == maxBuffers)
                        throw new InvalidOperationException("Exceeded maximum amount of buffers");

                    buffers[currentBufferIndex] = CreateVertexBuffer(bufferSize);
                    buffers[currentBufferIndex].Initialize();
                    availableBuffers++;
                }
            }
        }

        public void FlushBatch()
        {
            while (translucentVertices.Count > 0)
                AddVertex(translucentVertices.Pop());

            if (buffers[currentBufferIndex].Count > 0)
                buffers[currentBufferIndex].DrawBuffer();

            currentBufferIndex = 0;
        }

        protected abstract VertexBuffer<T> CreateVertexBuffer(int size);

        public void Dispose()
        {
            foreach (VertexBuffer<T> buffer in buffers)
                buffer?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
