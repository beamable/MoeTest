using Beamable.Common.Content;

namespace Custom_Content
{
    [ContentType("TestData01")]
    public class TestData01 : ContentObject
    {
        public string name;
    }
    
    [System.Serializable]
    public class TestData01Ref : ContentRef<TestData01> {}
    
}