// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

#if UNITY_EDITOR

namespace AxonGenesis
{
    public class PropertyMenuItem
    {
        public AxonGenesisBehavior Owner;
        public Property ToProperty;
        public Property FromProperty;

        public bool SeparateChannels;
        public bool IsUniformValue;

        public PropertyMenuItem(AxonGenesisBehavior owner, Property assignTo, Property fromProp, int attribute, bool uniform, bool separate)
        {
            Owner = owner;
            ToProperty = assignTo;
            FromProperty = fromProp;
            IsUniformValue = uniform;
            fromProp.IsUniformValue = uniform;
            fromProp.Attribute = attribute;
            SeparateChannels = separate;

        }

        public PropertyMenuItem(PropertyMenuItem copy)
        {
            Owner = copy.Owner;
            ToProperty = copy.ToProperty;
            FromProperty = copy.FromProperty;
            IsUniformValue = copy.IsUniformValue;
            SeparateChannels = copy.SeparateChannels;
        }
    }

}//AxonGenesis
#endif