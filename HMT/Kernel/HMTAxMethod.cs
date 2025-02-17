using Microsoft.Dynamics.AX.Metadata.MetaModel;

namespace HMT.Kernel
{
    public class HMTAxMethod : AxMethod 
    {
        public string GenerateMethod()
        {
            return "";
        }
    }

    public interface IAxMethodBuilder
    {
        HMTAxMethod build();
    }

    public class AxMethodBuilder : IAxMethodBuilder
    {
        private HMTAxMethod HMTAxMethod;

        public AxMethodBuilder(string name)
        {
            HMTAxMethod = new HMTAxMethod();

            HMTAxMethod.Name = name;
        }

        public AxMethodBuilder SetSource(string source)
        {
            HMTAxMethod.Source = source;

            return this;
        }

        public HMTAxMethod build()
        {
            return HMTAxMethod;
        }
    }
}
