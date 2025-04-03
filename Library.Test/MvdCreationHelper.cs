using System;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.MvdXml;

namespace Tests
{
    class MvdCreationHelper
    {
        public static mvdXML GetRequirementsMvd()
        {
            var schemas = new[] { "IFC2X3", "IFC4" };

            // uuids for references
            var objectsAndTypesPsetsUuid = "975A5C4A-43BD-4B1B-AD34-777E94DEBA85".ToLowerInvariant();
            var psetsUuid = "C85267BA-90C0-48F2-8739-72C2D45B70C9".ToLowerInvariant();
            var singleValueUuid = "421B209D-C6E9-4038-A8AA-CCB78DD91C53".ToLowerInvariant();
            var exchangeRequirementUuid = "00812FB8-0B86-4412-85AC-7EB0F5B7479E".ToLowerInvariant();

            var mvd = new mvdXML
            {
                uuid = Guid.NewGuid().ToString().ToLowerInvariant(),
                name = "example 7.2",
                Templates = new[] {
                    new ConceptTemplate {
                        uuid = objectsAndTypesPsetsUuid,
                        name= "Property Sets for Objects and Types",
                        applicableSchema = schemas,
                        applicableEntity = new[] { nameof(IfcObject) },
                        Rules = new[] {
                            new AttributeRule {
                                AttributeName = nameof(IfcObject.IsDefinedBy),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule{
                                            EntityName = nameof(IfcRelDefinesByProperties),
                                            AttributeRules = new EntityRuleAttributeRules{
                                                AttributeRule = new []{
                                                    new AttributeRule {
                                                        AttributeName = nameof(IfcRelDefinesByProperties.RelatingPropertyDefinition),
                                                        EntityRules = new AttributeRuleEntityRules{
                                                            EntityRule = new []{
                                                                new EntityRule {
                                                                    EntityName = nameof(IfcPropertySet),
                                                                    References = new EntityRuleReferences{
                                                                        IdPrefix = "O_",
                                                                        Template = new GenericReference{ @ref = psetsUuid}
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            new AttributeRule {
                                AttributeName = nameof(IfcObject.IsTypedBy),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule {
                                            EntityName = nameof(IfcRelDefinesByType),
                                            AttributeRules = new EntityRuleAttributeRules{
                                                AttributeRule = new []{
                                                    new AttributeRule{
                                                        AttributeName = nameof(IfcRelDefinesByType.RelatingType),
                                                        EntityRules = new AttributeRuleEntityRules {
                                                            EntityRule = new []{
                                                                new EntityRule{
                                                                    EntityName = nameof(IfcTypeObject),
                                                                    AttributeRules = new EntityRuleAttributeRules {
                                                                        AttributeRule = new []{
                                                                            new AttributeRule {
                                                                                AttributeName = nameof(IfcTypeObject.HasPropertySets),
                                                                                EntityRules = new AttributeRuleEntityRules{
                                                                                    EntityRule = new []{
                                                                                        new EntityRule{
                                                                                            EntityName = nameof(IfcPropertySet),
                                                                                            References = new EntityRuleReferences {
                                                                                                IdPrefix = "T_",
                                                                                                Template = new GenericReference{ @ref = psetsUuid}
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new ConceptTemplate {
                        uuid = singleValueUuid,
                        name = "Single value",
                        applicableSchema = schemas,
                        applicableEntity = new []{ nameof(IfcPropertySingleValue) },
                        isPartial = true,
                        Rules = new []{
                            new AttributeRule{
                                RuleID = "PName",
                                AttributeName = nameof(IfcPropertySingleValue.Name),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule{ EntityName = nameof(IfcIdentifier)}
                                    }
                                }
                            },
                            new AttributeRule{
                                RuleID = "PDescription",
                                AttributeName = nameof(IfcPropertySingleValue.Description),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule{ EntityName = nameof(IfcText)}
                                    }
                                }
                            },
                            new AttributeRule{
                                RuleID = "PSingleValue",
                                AttributeName = nameof(IfcPropertySingleValue.NominalValue),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule{ EntityName = nameof(IfcValue)}
                                    }
                                }
                            },
                        }
                    },
                    new ConceptTemplate{
                        uuid = psetsUuid,
                        name = "Property Sets",
                        applicableSchema = schemas,
                        applicableEntity = new []{nameof(IfcPropertySet) },
                        isPartial = true,
                        Rules = new []{
                            new AttributeRule {
                                RuleID = "PsetName",
                                AttributeName = nameof(IfcPropertySet.Name),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule { EntityName = nameof(IfcLabel)}
                                    }
                                }
                            },
                            new AttributeRule{
                                RuleID = "PsetDescription",
                                AttributeName = nameof(IfcPropertySet.Description),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new []{
                                        new EntityRule{ EntityName = nameof(IfcText)}
                                    }
                                }
                            },
                            new AttributeRule{
                                AttributeName = nameof(IfcPropertySet.HasProperties),
                                EntityRules = new AttributeRuleEntityRules{
                                    EntityRule = new[]{
                                        new EntityRule {
                                            EntityName = nameof(IfcPropertySingleValue),
                                            References = new EntityRuleReferences{
                                                Template = new GenericReference{ @ref = singleValueUuid}
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Views = new[] {
                    new ModelView{
                        applicableSchema = "IFC4",
                        uuid = Guid.NewGuid().ToString().ToLowerInvariant(),
                        name = "design phase",
                        code = "LPH 3",
                        Definitions = new []{
                            new DefinitionsDefinition{
                                Body = new DefinitionsDefinitionBody{ lang = "en", Value = "Definition of the view in english"}
                            }
                        },
                        ExchangeRequirements = new []{
                            new ModelViewExchangeRequirement{
                                uuid = exchangeRequirementUuid,
                                name = "design plase coordination",
                                code = "LPH 3a",
                                applicability = applicability.export,
                                Definitions = new []{
                                    new DefinitionsDefinition{
                                        Body = new DefinitionsDefinitionBody{ lang= "en", Value = "Definition of the exchange requirement in english"}
                                    }
                                }
                            }
                        },
                        Roots = new []{
                            new ConceptRoot{
                                uuid = Guid.NewGuid().ToString().ToLowerInvariant(),
                                name = "load bearing external walls",
                                applicableRootEntity = nameof(IfcWall),
                                Definitions = new []{
                                    new DefinitionsDefinition{
                                        Body = new DefinitionsDefinitionBody{ lang = "en", Value = "Definition of the view in english"}
                                    }
                                },
                                Applicability = new ConceptRootApplicability{
                                    Template = new GenericReference{ @ref = objectsAndTypesPsetsUuid },
                                    // IsExternal AND LoadBearing
                                    TemplateRules = new TemplateRules{
                                         @operator = TemplateRulesOperator.and,
                                         Items = new [] {
                                             new TemplateRules{
                                                 @operator = TemplateRulesOperator.or,
                                                 Items = new object []{
                                                     new TemplateRulesTemplateRule {
                                                         Parameters = "O_PsetName[Value]='Pset_WallCommon' AND O_PName[Value]='IsExternal' AND O_PSingleValue[Value]=TRUE"
                                                     },
                                                     new TemplateRules {
                                                         @operator = TemplateRulesOperator.and,
                                                         Items = new object[]{
                                                             new TemplateRulesTemplateRule{
                                                                 Parameters = "T_PsetName[Value]='Pset_WallCommon' AND T_PName[Value]='IsExternal' AND T_PSingleValue[Value]=TRUE"
                                                             },
                                                             new TemplateRules{
                                                                 @operator = TemplateRulesOperator.not,
                                                                 Items = new []{
                                                                     new TemplateRulesTemplateRule{
                                                                         Parameters = "O_PsetName[Value]='Pset_WallCommon' AND O_PName[Value]='IsExternal'"
                                                                     }
                                                                 }
                                                             }
                                                         }
                                                     }

                                                 }
                                             },
                                             new TemplateRules{
                                                 @operator = TemplateRulesOperator.or,
                                                 Items = new object []{
                                                     new TemplateRulesTemplateRule {
                                                         Parameters = "O_PsetName[Value]='Pset_WallCommon' AND O_PName[Value]='LoadBearing' AND O_PSingleValue[Value]=TRUE"
                                                     },
                                                     new TemplateRules {
                                                         @operator = TemplateRulesOperator.and,
                                                         Items = new object[]{
                                                             new TemplateRulesTemplateRule{
                                                                 Parameters = "T_PsetName[Value]='Pset_WallCommon' AND T_PName[Value]='LoadBearing' AND T_PSingleValue[Value]=TRUE"
                                                             },
                                                             new TemplateRules{
                                                                 @operator = TemplateRulesOperator.not,
                                                                 Items = new []{
                                                                     new TemplateRulesTemplateRule{
                                                                         Parameters = "O_PsetName[Value]='Pset_WallCommon' AND O_PName[Value]='LoadBearing'"
                                                                     }
                                                                 }
                                                             }
                                                         }
                                                     }

                                                 }
                                             }
                                         }
                                    }
                                },
                                Concepts = new []{
                                    // check existance of FireRating property
                                    new Concept {
                                        uuid = Guid.NewGuid().ToString().ToLowerInvariant(),
                                        name = "load bearing external walls required to have property 'FireRating'",
                                         Definitions = new []{
                                            new DefinitionsDefinition{
                                                Body = new DefinitionsDefinitionBody{ lang = "en", Value = "Load bearing external walls required to have property 'FireRating'"}
                                            }
                                        },
                                         Template = new GenericReference{@ref = objectsAndTypesPsetsUuid},
                                         Requirements = new [] {
                                            new RequirementsRequirement {
                                                applicability = applicability.both,
                                                requirement = RequirementsRequirementRequirement.mandatory,
                                                exchangeRequirement = exchangeRequirementUuid
                                            }
                                         },
                                         TemplateRules = new TemplateRules{
                                             @operator = TemplateRulesOperator.or,
                                             Items = new []{
                                                 new TemplateRulesTemplateRule {
                                                     Parameters = "O_PsetName[Value]='Pset_WallCommon' AND O_PName[Value]='FireRating' AND O_PSingleValue[Exists]=TRUE"
                                                 },
                                                 new TemplateRulesTemplateRule {
                                                     Parameters = "T_PsetName[Value]='Pset_WallCommon' AND T_PName[Value]='FireRating' AND T_PSingleValue[Exists]=TRUE"
                                                 }
                                             }
                                         }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            return mvd;
        }
    }
}
