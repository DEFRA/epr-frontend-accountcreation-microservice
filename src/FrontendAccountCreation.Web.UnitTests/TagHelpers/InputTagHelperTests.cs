using FrontendAccountCreation.Web.TagHelpers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;

namespace FrontendAccountCreation.Web.UnitTests.TagHelpers
{
    [TestClass]
    public class InputTagHelperTests
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
        public void testProcess_withIsFirstOptionFalse()
        {
            var inputTagHelper = new InputTagHelper();
            inputTagHelper.IsFirstOption = false;
            inputTagHelper.Value = "Test";
            inputTagHelper.Type = "radio";

            ModelExpression me = new ModelExpression("ModelExp1", _modelExplorer.Object);
            inputTagHelper.For = me;
            var tagHelperContext = new TagHelperContext(
                            new TagHelperAttributeList()
                            {
                new TagHelperAttribute("gov-value","value"),
                new TagHelperAttribute("gov-first-option",  "false"),
                new TagHelperAttribute("gov-for",  me),
                },
                            new Dictionary<object, object>(),
                            Guid.NewGuid().ToString("N"));
            var tagHelperOutput = new TagHelperOutput("input",
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
            inputTagHelper.Process(tagHelperContext, tagHelperOutput);
            Assert.AreEqual("", tagHelperOutput.Content.GetContent());
            Assert.AreEqual("ModelExp1-Test", tagHelperOutput.Attributes[3].Value);
     

        }
        [TestMethod]
        public void testProcess_withIsFirstOptionTrue()
        {
            var inputTagHelper = new InputTagHelper();
            inputTagHelper.IsFirstOption = true;
            inputTagHelper.Value = "Test";
            inputTagHelper.Type = "radio";

            ModelExpression me = new ModelExpression("ModelExp1", _modelExplorer.Object);
            inputTagHelper.For = me;
            var tagHelperContext = new TagHelperContext(
                            new TagHelperAttributeList()
                            {
                new TagHelperAttribute("gov-value","value"),
                new TagHelperAttribute("gov-first-option",  "true"),
                new TagHelperAttribute("gov-for",  me),
                },
                            new Dictionary<object, object>(),
                            Guid.NewGuid().ToString("N"));
            var tagHelperOutput = new TagHelperOutput("input",
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
            inputTagHelper.Process(tagHelperContext, tagHelperOutput);

            Assert.AreEqual("", tagHelperOutput.Content.GetContent());
            Assert.AreEqual("ModelExp1", tagHelperOutput.Attributes[3].Value);
            Assert.AreEqual("checked", tagHelperOutput.Attributes[7].Value);

        }
    }
}
