/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using l4p.VcallModel.Utils;
using NUnit.Framework;

namespace l4p.VcallTests.Core
{
    [TestFixture]
    class FillPropertiesOfTests
    {
        class UnsopportedConfig
        {
            public short ShortField { get; set; }
        }

        class NonExistingPropertyConfig
        {
            public int NonExistingProperty { get; set; }
        }

        class Config
        {
            public int OneField { get; set; }
            public int? SecondField { get; set; }
            public string Name { get; set; }
            public string Family { get; set; }

            public SubConfig SubConfig { get; set; }
        }

        class SubConfig
        {
            public double SubField { get; set; }
            public double? SubField2 { get; set; }
        }

        class OtherConfig
        {
            public int OneField { get; set; }
            public int? SecondField { get; set; }
            public double SubField { get; set; }
            public double? SubField2 { get; set; }
            public string Name { get; set; }
        }

        class SameNamesInConfig
        {
            public double SubField { get; set; }
            public SubConfig SubConfig { get; set; }
        }

        [Test]
        public void CopyConfiguration_should_do_the_job()
        {
            var config = new Config
            {
                OneField = 123,
                SecondField = 321,
                Name = "Mamasha",
                Family = "Knows",
                SubConfig = new SubConfig()
                {
                    SubField = 1.23,
                    SubField2 = 3.21
                }
            };

            var other = FillPropertiesOf<OtherConfig>.From(config);

            Assert.That(other.OneField, Is.EqualTo(123));
            Assert.That(other.SecondField, Is.EqualTo(321));
            Assert.That(other.SubField, Is.EqualTo(1.23));
            Assert.That(other.SubField2, Is.EqualTo(3.21));
            Assert.That(other.Name, Is.EqualTo("Mamasha"));
        }

        [Test, ExpectedException(typeof(FillPropertiesOfException))]
        public void CopyUnsupportedTypes_should_die()
        {
            var config = new UnsopportedConfig();
            FillPropertiesOf<OtherConfig>.From(config);
        }

        [Test, ExpectedException(typeof(FillPropertiesOfException))]
        public void FillNonExistingProperties_should_die()
        {
            var config = new Config
            {
                SubConfig = new SubConfig()
            };

            FillPropertiesOf<NonExistingPropertyConfig>.From(config);
        }

        [Test, ExpectedException(typeof(FillPropertiesOfException))]
        public void CopyWrongConfig_should_die()
        {
            var config = new SameNamesInConfig()
            {
                SubConfig = new SubConfig()
            };

            FillPropertiesOf<OtherConfig>.From(config);
        }
    }
}