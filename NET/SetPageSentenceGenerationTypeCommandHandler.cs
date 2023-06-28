using System.Threading.Tasks;
using Abstract.Helpful.Lib;
using Domain.Logic.Transport;
using Domain.Types._Page.Services.Storage;
using Domain.Types._Site.Services.Storage;
using Domain.Types.DomainModels._User.Services;
using Domain.Types.Entities.Generated;
using Domain.Types.Exceptions;
using Domain.Types.Extensions;
using Domain.Types.SharedBusinessLogic.SentenceGenerator.Logic;
using Domain.Types.SharedBusinessLogic.SentenceGenerator.Logic.AsyncChanges;
using Domain.Types.SharedInfrastructure.CQRS.Messages.Commands;
using Domain.Types.SharedInfrastructure.CQRS.Messages.Commands.Custom;
using Domain.Types.Types.LanguageDictionary_Domain;
using SeoGenie.Cqrs.CommandHandlers.Sources.Base;
using Serilog;

namespace SeoGenie.Cqrs.CommandHandlers.Sources._Metadata
{
    // ReSharper disable once UnusedType.Global
    public sealed class SetPageSentenceGenerationTypeCommandHandler
        : UnifiedCommandHandlerBase<SetPageSentenceGenerationTypeCommand>
    {
        private readonly AccessValidator _accessValidator;
        private readonly MetadataAsyncOnChangesRegenerator _metadataAsyncOnChangesRegenerator;
        private readonly IPageStorage _pageStorage;
        private readonly ISiteStorageReadonly _siteStorageReadonly;
        private readonly PageStaticStorage _pageStaticStorage;
        
        public SetPageSentenceGenerationTypeCommandHandler(
            IEventPublisher eventPublisher,
            ILogger logger, 
            AccessValidator accessValidator, 
            MetadataAsyncOnChangesRegenerator metadataAsyncOnChangesRegenerator,
            IPageStorage pageStorage, 
            ISiteStorageReadonly siteStorageReadonly, 
            PageStaticStorage pageStaticStorage)
            : base(eventPublisher, logger)
        {
            _accessValidator = accessValidator;
            _metadataAsyncOnChangesRegenerator = metadataAsyncOnChangesRegenerator;
            _pageStorage = pageStorage;
            _siteStorageReadonly = siteStorageReadonly;
            _pageStaticStorage = pageStaticStorage;
        }

        protected override async Task HandleInner(SetPageSentenceGenerationTypeCommand command)
        {
            // Validating
        
            var pageId = command.PageId.ToPageIdOrThrow();
            var generationType = command.SentenceGenerationType;
            var sentenceType = command.SentenceType;
            
            generationType.ValidateRequiredNotInvalid(SentenceGenerationType.Invalid, nameof(generationType));
            sentenceType.ValidateRequiredNotInvalid(SentenceType.Invalid, nameof(sentenceType));

            await _accessValidator.Validate(command);

            // Processing
            
            var page = await _pageStaticStorage.GetById(pageId);
            var generatorSettings = await _pageStorage.GetGeneratorSettingsOrDefault(pageId);
            if (generatorSettings.IsDefault())
                generatorSettings = await _siteStorageReadonly.GetGeneratorSettings(page.SiteId);
            generatorSettings = generatorSettings.ReplaceIfDefault(GeneratorSettings.Default);
            generatorSettings.ApplyChange(sentenceType, generationType);
            
            // Saving
            
            await _pageStorage.SetGeneratorSettings(pageId, generatorSettings, false);
            
            // Events invoking
            
            var sentenceGenerationTypeChange = new SentenceGenerationTypeChange(command.SentenceType, 
                command.SentenceGenerationType);
            
            var metadataGeneratorInputsChange = new MetadataGeneratorInputsChange(sentenceGenerationTypeChange, pageId);
            
            _metadataAsyncOnChangesRegenerator.OnChange(metadataGeneratorInputsChange);
        }
    }
}