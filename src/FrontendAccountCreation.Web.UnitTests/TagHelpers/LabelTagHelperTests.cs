using FrontendAccountCreation.Web.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace FrontendAccountCreation.Web.UnitTests.TagHelpers
{
    [TestClass]
    public class LabelTagHelperTests
   {
        ModelMetadataIdentity _identity = ModelMetadataIdentity.ForType(typeof(String));
        Mock<ModelExplorer> _modelExplorer = null;
        Mock<IModelMetadataProvider> _metadataProvider = null;
        Mock<ModelMetadata> _metadata = null;


        [TestInitialize]
        public void Setup()
        {
            _metadataProvider = new Mock<IModelMetadataProvider>();
            _metadata = new Mock<ModelMetadata>(_identity);
            _modelExplorer = new Mock<ModelExplorer>(_metadataProvider.Object, _metadata.Object, new String("Test"));

        }

        [TestMethod]
        public void testProcess_withIsFirstOption_false()
        {
            var labelTagHelper = new LabelTagHelper();
            labelTagHelper.IsFirstOption = false;
            labelTagHelper.Value = "Test";

            ModelExpression me = new ModelExpression("ModelExp1", _modelExplorer.Object);
            labelTagHelper.For = me;
            var tagHelperContext = new TagHelperContext(
                            new TagHelperAttributeList()
                            {
                new TagHelperAttribute("gov-value","value"),
                new TagHelperAttribute("gov-first-option",  "false"),
                new TagHelperAttribute("gov-for",  me),
                },
                            new Dictionary<object, object>(),
                            Guid.NewGuid().ToString("N"));
            var tagHelperOutput = new TagHelperOutput("label",
                new TagHelperAttributeList() {
                new TagHelperAttribute("gov-value","value"),
                new TagHelperAttribute("gov-first-option",  "false"),
                new TagHelperAttribute("gov-for",  me),
            },
            (result, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetHtmlContent(string.Empty);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
            labelTagHelper.Process(tagHelperContext, tagHelperOutput);
        
            Assert.AreEqual("", tagHelperOutput.Content.GetContent());
            Assert.AreEqual(4, tagHelperOutput.Attributes.Count);
            Assert.AreEqual(4, tagHelperOutput.Attributes.Count);
            Assert.AreEqual("ModelExp1-Test", tagHelperOutput.Attributes[3].Value);

        }
        [TestMethod]
        public void testProcess_withIsFirstOption_true()
        {
            var labelTagHelper = new LabelTagHelper();
            labelTagHelper.IsFirstOption = true;
            labelTagHelper.Value = "Test";

            ModelExpression me = new ModelExpression("ModelExp1", _modelExplorer.Object);
            labelTagHelper.For = me;
            var tagHelperContext = new TagHelperContext(
                            new TagHelperAttributeList()
                            {
                new TagHelperAttribute("gov-value","value"),
                new TagHelperAttribute("gov-first-option",  "true"),
                new TagHelperAttribute("gov-for",  me),
                },
                            new Dictionary<object, object>(),
                            Guid.NewGuid().ToString("N"));
            var tagHelperOutput = new TagHelperOutput("label",
                new TagHelperAttributeList() {
                new TagHelperAttribute("gov-value","value"),
                new TagHelperAttribute("gov-first-option",  "true"),
                new TagHelperAttribute("gov-for",  me),
            },
            (result, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                tagHelperContent.SetHtmlContent(string.Empty);
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
            labelTagHelper.Process(tagHelperContext, tagHelperOutput);


            Assert.AreEqual("", tagHelperOutput.Content.GetContent());
            Assert.AreEqual(4, tagHelperOutput.Attributes.Count);
            Assert.AreEqual(4, tagHelperOutput.Attributes.Count);
            Assert.AreEqual("ModelExp1", tagHelperOutput.Attributes[3].Value);

        }
    }
}
