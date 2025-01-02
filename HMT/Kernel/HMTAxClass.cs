using Microsoft.Dynamics.AX.Metadata.MetaModel;
using System.Collections.Generic;

namespace HMT.Kernel
{
    public class HMTAxClass : AxClass 
    {

    }

    public interface IBuilder
    {
        HMTAxClass build();
    }

    /// <summary>
    /// HMT.Kernel
    /// Willie Yao - 04/29/2024
    /// Builder class for HMTAxClass
    /// </summary>
    public class Builder : IBuilder
    {
        private HMTAxClass HMTAxClass;

        public Builder(string name)
        {
            HMTAxClass = new HMTAxClass();

            HMTAxClass.Name = name;
        }

        public Builder SetInternal(bool isInternal)
        {
            HMTAxClass.IsInternal = isInternal;

            return this;
        }

        public Builder SetIsPrivate(bool isPrivate)
        {
            HMTAxClass.IsPrivate = isPrivate;

            return this;
        }

        public Builder SetIsPublic(bool isPublic)
        {
            HMTAxClass.IsPublic = isPublic;

            return this;
        }

        public Builder SetDeclaration(string declaration)
        {
            HMTAxClass.Declaration = declaration;

            return this;
        }

        public Builder SetExtends(string extends)
        {
            HMTAxClass.Extends = extends;

            return this;
        }

        public Builder SetImplements(List<string> implements)
        {
            HMTAxClass.Implements = implements;

            return this;
        }

        public Builder SetIsAbstract(bool isAbstract)
        {
            HMTAxClass.IsAbstract = isAbstract;

            return this;
        }

        public Builder SetIsFinal(bool isFinal)
        {
            HMTAxClass.IsFinal = isFinal;

            return this;
        }

        public Builder SetIsInterface(bool isInterface)
        {
            HMTAxClass.IsInterface = isInterface;

            return this;
        }

        public Builder SetIsStatic(bool isStatic)
        {
            HMTAxClass.IsStatic = isStatic;

            return this;
        }

        public Builder SetMembers(List<AxClassMemberVariable> members)
        {
            HMTAxClass.Members = members;

            return this;
        }

        public HMTAxClass build()
        {
            return HMTAxClass;
        }
    }
}