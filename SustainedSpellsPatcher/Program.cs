using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using DynamicData;
using Noggog;

namespace SustainedSpellsPatcher
{
    public class TestSettings
    {
        //public double InitialCostMultiplier = 0.0;

        //public double LastingCostMultiplier = 1.0;

        public bool enableSummonSpells = true;
        public bool enableReanimateSpells = true;
        public bool enableBoundWeaponSpells = true;
        public bool enableCloakSpells = true;
        public bool enableFleshSpells = true;
        public bool enableShieldSpells = true;
        public bool enableFeatherSpells = true;
        public bool enableWaterbreathingSpells = true;
        public bool enableCandlelightSpells = true;
        //public bool enableMagelightSpells = true;
        public bool enableMuffleSpells = true;

        public bool replaceSpells = true;

        public List<IFormLinkGetter<ISpellGetter>> blacklist = new();

    }

    public class Program
    {
        public enum SpellArchetypeType
        {
            Unknown,
            BoundWeapon,
            Summon,
            Reanimate,
            Flesh,
            Waterbreathing,
            Feather,
            Muffle,
            Cloak,
            SoulCloak,
            Shield,
            Candlelight
            //Magelight
        }

        public static SpellArchetypeType GetSpellType(IMagicEffectGetter magicEffect)
        {
            switch (magicEffect.Archetype.Type)
            {
                case MagicEffectArchetype.TypeEnum.SummonCreature:
                    return SpellArchetypeType.Summon;
                case MagicEffectArchetype.TypeEnum.Reanimate:
                    return SpellArchetypeType.Reanimate;
                case MagicEffectArchetype.TypeEnum.Bound:
                    return SpellArchetypeType.BoundWeapon;
                case MagicEffectArchetype.TypeEnum.Cloak:
                    if (magicEffect.Keywords != null)
                    {
                        if (magicEffect.Keywords.Contains(FormKey.Factory("0B62E4:Skyrim.esm")))
                        {
                            return SpellArchetypeType.Cloak;
                        }
                        else if (magicEffect.Keywords.Contains(FormKey.Factory("F405A2:MysticismMagic.esm")))
                        {
                            return SpellArchetypeType.SoulCloak;
                        }
                    }
                    break;
                case MagicEffectArchetype.TypeEnum.Light:
                    if (magicEffect.TargetType == TargetType.Self)
                    {
                        return SpellArchetypeType.Candlelight;
                    }
                    /*
                    else if (magicEffect.TargetType == TargetType.Aimed)
                    {
                        return SpellArchetypeType.Magelight;
                    }
                    */
                    break;
                case MagicEffectArchetype.TypeEnum.PeakValueModifier:
                    if (magicEffect.Archetype.ActorValue == ActorValue.WaterBreathing)
                    {
                        return SpellArchetypeType.Waterbreathing;
                    } else if (magicEffect.Archetype.ActorValue == ActorValue.MovementNoiseMult)
                    {
                        return SpellArchetypeType.Muffle;
                    }
                    else if (magicEffect.Keywords != null)
                    {
                        if (magicEffect.Keywords.Contains(FormKey.Factory("01EA72:Skyrim.esm")))
                        {
                            return SpellArchetypeType.Flesh;
                        }
                        else if (magicEffect.Keywords.Contains(FormKey.Factory("292681:MysticismMagic.esp")) || magicEffect.Keywords.Contains(FormKey.Factory("2849EA:Odin - Skyrim Magic Overhaul.esp")))
                        {
                            return SpellArchetypeType.Feather;
                        }
                        else if (magicEffect.Keywords.Contains(FormKey.Factory("274031:MysticismMagic.esp")))
                        {
                            return SpellArchetypeType.Shield;
                        }
                    }
                    break;
            }
            return SpellArchetypeType.Unknown;
        }

        public static bool IsSpellTypeEnabled(SpellArchetypeType spellType)
        {
            switch (spellType)
            {
                case SpellArchetypeType.BoundWeapon:
                    return Settings.Value.enableBoundWeaponSpells;
                case SpellArchetypeType.Summon:
                    return Settings.Value.enableSummonSpells;
                case SpellArchetypeType.Flesh:
                    return Settings.Value.enableFleshSpells;
                case SpellArchetypeType.Cloak:
                    return Settings.Value.enableCloakSpells;
                case SpellArchetypeType.SoulCloak:
                    return Settings.Value.enableCloakSpells;
                case SpellArchetypeType.Feather:
                    return Settings.Value.enableFeatherSpells;
                case SpellArchetypeType.Muffle:
                    return Settings.Value.enableMuffleSpells;
                case SpellArchetypeType.Reanimate:
                    return Settings.Value.enableReanimateSpells;
                case SpellArchetypeType.Shield:
                    return Settings.Value.enableShieldSpells;
                case SpellArchetypeType.Waterbreathing:
                    return Settings.Value.enableWaterbreathingSpells;
                case SpellArchetypeType.Candlelight:
                    return Settings.Value.enableCandlelightSpells;
                    /*
                case SpellArchetypeType.Magelight:
                    return Settings.Value.enableMagelightSpells;
                    */
                case SpellArchetypeType.Unknown:
                    return false;
            }
            return false;
        }


        static Lazy<TestSettings> Settings = null!;
        public static string CapatalizeFirst(string text)
        {
            return string.Concat(text[0].ToString().ToUpper(), text.AsSpan(1));
        }

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetAutogeneratedSettings(
                    nickname: "Settings",
                    path: "settings.json",
                    out Settings)
                .SetTypicalOpen(GameRelease.SkyrimSE, "YourPatcher.esp")
                .Run(args);
        }


        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            //Your code here!

            List<IFormLinkGetter<ISpellGetter>> blacklist = Settings.Value.blacklist;
            if (!state.LoadOrder.ModExists(new ModKey("MysticismMagic", ModType.Plugin), true))
            {
                blacklist.Add(FormKey.Factory("07E5D5:Skyrim.esm").ToLinkGetter<ISpellGetter>()); // Flame Thrall
                blacklist.Add(FormKey.Factory("07E5D6:Skyrim.esm").ToLinkGetter<ISpellGetter>()); // Frost Thrall
                blacklist.Add(FormKey.Factory("07E5D7:Skyrim.esm").ToLinkGetter<ISpellGetter>()); // Storm Thrall
            }
            blacklist.Add(FormKey.Factory("07E8DF:Skyrim.esm").ToLinkGetter<ISpellGetter>()); // Dead Thrall
            blacklist.Add(FormKey.Factory("289AFB:Odin - Skyrim Magic Overhaul.esp").ToLinkGetter<ISpellGetter>()); // Orc Strength

            FormList drainSpellsListConjuration = new(state.PatchMod, "SustainedSpellsDrainListConjuration");
            FormList baseSpellsListConjuration = new(state.PatchMod, "SustainedSpellsBaseListConjuration");
            FormList lastingSpellsListConjuration = new(state.PatchMod, "SustainedSpellsEffectListConjuration");
            FormList lastingSpellTrackerListConjuration = new(state.PatchMod, "SustainedSpellsTrackerListConjuration");
            FormList drainSpellsListDestruction = new(state.PatchMod, "SustainedSpellsDrainListDestruction");
            FormList baseSpellsListDestruction = new(state.PatchMod, "SustainedSpellsBaseListDestruction");
            FormList lastingSpellsListDestruction = new(state.PatchMod, "SustainedSpellsEffectListDestruction");
            FormList lastingSpellTrackerListDestruction = new(state.PatchMod, "SustainedSpellsTrackerListDestruction");
            FormList drainSpellsListRestoration = new(state.PatchMod, "SustainedSpellsDrainListRestoration");
            FormList baseSpellsListRestoration = new(state.PatchMod, "SustainedSpellsBaseListRestoration");
            FormList lastingSpellsListRestoration = new(state.PatchMod, "SustainedSpellsEffectListRestoration");
            FormList lastingSpellTrackerListRestoration = new(state.PatchMod, "SustainedSpellsTrackerListRestoration");
            FormList drainSpellsListAlteration = new(state.PatchMod, "SustainedSpellsDrainListAlteration");
            FormList baseSpellsListAlteration = new(state.PatchMod, "SustainedSpellsBaseListAlteration");
            FormList lastingSpellsListAlteration = new(state.PatchMod, "SustainedSpellsEffectListAlteration");
            FormList lastingSpellTrackerListAlteration = new(state.PatchMod, "SustainedSpellsTrackerListAlteration");
            FormList drainSpellsListIllusion = new(state.PatchMod, "SustainedSpellsDrainListIllusion");
            FormList baseSpellsListIllusion = new(state.PatchMod, "SustainedSpellsBaseListIllusion");
            FormList lastingSpellsListIllusion = new(state.PatchMod, "SustainedSpellsEffectListIllusion");
            FormList lastingSpellTrackerListIllusion = new(state.PatchMod, "SustainedSpellsTrackerListIllusion");


            foreach (var magicEffectGetter in state.LoadOrder.PriorityOrder.MagicEffect().WinningOverrides())
            {
                if (magicEffectGetter.Flags.HasFlag(MagicEffect.Flag.Recover) && (magicEffectGetter.Archetype.Type == MagicEffectArchetype.TypeEnum.ValueModifier || magicEffectGetter.Archetype.Type == MagicEffectArchetype.TypeEnum.PeakValueModifier || magicEffectGetter.Archetype.Type == MagicEffectArchetype.TypeEnum.DualValueModifier))
                {
                    var magicEffect = magicEffectGetter.DeepCopy();
                    magicEffect.VirtualMachineAdapter ??= new();
                    if (magicEffectGetter.Archetype.ActorValue == ActorValue.Magicka || magicEffectGetter.SecondActorValue == ActorValue.Magicka)
                    {
                        if (magicEffectGetter.Flags.HasFlag(MagicEffect.Flag.Detrimental))
                        {
                            magicEffect.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                            {
                                Flags = ScriptEntry.Flag.Local,
                                Name = "DispelSustainedSpellsStart",
                                Properties = new()
                                {
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "DispellSpell",
                                        Object = FormKey.Factory("000D6A:Sustained Spells.esp").ToLink<Spell>()
                                    }
                                }
                            });
                        }
                        else
                        {
                            magicEffect.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                            {
                                Flags = ScriptEntry.Flag.Local,
                                Name = "DispelSustainedSpellsFinish",
                                Properties = new()
                                {
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "DispellSpell",
                                        Object = FormKey.Factory("000D6A:Sustained Spells.esp").ToLink<Spell>()
                                    }
                                }
                            });
                        }
                        state.PatchMod.MagicEffects.Set(magicEffect);
                    }

                    switch (magicEffectGetter.Archetype.ActorValue)
                    {
                        case ActorValue.ConjurationModifier:
                            magicEffect.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                            {
                                Flags = ScriptEntry.Flag.Local,
                                Name = "SustainedSpellsRecalculateCosts",
                                Properties = new()
                                {
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsDrainList",
                                        Object = drainSpellsListConjuration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsBaseList",
                                        Object = baseSpellsListConjuration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsEffectList",
                                        Object = lastingSpellsListConjuration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsTrackerList",
                                        Object = lastingSpellTrackerListConjuration.ToLink()
                                    }
                                }
                            });
                            state.PatchMod.MagicEffects.Set(magicEffect);
                            break;
                        case ActorValue.IllusionModifier:
                            magicEffect.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                            {
                                Flags = ScriptEntry.Flag.Local,
                                Name = "SustainedSpellsRecalculateCosts",
                                Properties = new()
                                {
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsDrainList",
                                        Object = drainSpellsListIllusion.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsBaseList",
                                        Object = baseSpellsListIllusion.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsEffectList",
                                        Object = lastingSpellsListIllusion.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsTrackerList",
                                        Object = lastingSpellTrackerListIllusion.ToLink()
                                    }
                                }
                            });
                            state.PatchMod.MagicEffects.Set(magicEffect);
                            break;
                        case ActorValue.AlterationModifier:
                            magicEffect.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                            {
                                Flags = ScriptEntry.Flag.Local,
                                Name = "SustainedSpellsRecalculateCosts",
                                Properties = new()
                                {
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsDrainList",
                                        Object = drainSpellsListAlteration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsBaseList",
                                        Object = baseSpellsListAlteration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsEffectList",
                                        Object = lastingSpellsListAlteration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsTrackerList",
                                        Object = lastingSpellTrackerListAlteration.ToLink()
                                    }
                                }
                            });
                            state.PatchMod.MagicEffects.Set(magicEffect);
                            break;
                        case ActorValue.DestructionModifier:
                            magicEffect.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                            {
                                Flags = ScriptEntry.Flag.Local,
                                Name = "SustainedSpellsRecalculateCosts",
                                Properties = new()
                                {
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsDrainList",
                                        Object = drainSpellsListDestruction.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsBaseList",
                                        Object = baseSpellsListDestruction.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsEffectList",
                                        Object = lastingSpellsListDestruction.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsTrackerList",
                                        Object = lastingSpellTrackerListDestruction.ToLink()
                                    }
                                }
                            });
                            state.PatchMod.MagicEffects.Set(magicEffect);
                            break;
                        case ActorValue.RestorationModifier:
                            magicEffect.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                            {
                                Flags = ScriptEntry.Flag.Local,
                                Name = "SustainedSpellsRecalculateCosts",
                                Properties = new()
                                {
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsDrainList",
                                        Object = drainSpellsListRestoration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsBaseList",
                                        Object = baseSpellsListRestoration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsEffectList",
                                        Object = lastingSpellsListRestoration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsTrackerList",
                                        Object = lastingSpellTrackerListRestoration.ToLink()
                                    }
                                }
                            });
                            state.PatchMod.MagicEffects.Set(magicEffect);
                            break;
                    }
                    switch (magicEffectGetter.SecondActorValue)
                    {
                        case ActorValue.ConjurationModifier:
                            magicEffect.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                            {
                                Flags = ScriptEntry.Flag.Local,
                                Name = "SustainedSpellsRecalculateCosts",
                                Properties = new()
                                {
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsDrainList",
                                        Object = drainSpellsListConjuration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsBaseList",
                                        Object = baseSpellsListConjuration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsEffectList",
                                        Object = lastingSpellsListConjuration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsTrackerList",
                                        Object = lastingSpellTrackerListConjuration.ToLink()
                                    }
                                }
                            });
                            state.PatchMod.MagicEffects.Set(magicEffect);
                            break;
                        case ActorValue.IllusionModifier:
                            magicEffect.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                            {
                                Flags = ScriptEntry.Flag.Local,
                                Name = "SustainedSpellsRecalculateCosts",
                                Properties = new()
                                {
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsDrainList",
                                        Object = drainSpellsListIllusion.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsBaseList",
                                        Object = baseSpellsListIllusion.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsEffectList",
                                        Object = lastingSpellsListIllusion.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsTrackerList",
                                        Object = lastingSpellTrackerListIllusion.ToLink()
                                    }
                                }
                            });
                            state.PatchMod.MagicEffects.Set(magicEffect);
                            break;
                        case ActorValue.AlterationModifier:
                            magicEffect.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                            {
                                Flags = ScriptEntry.Flag.Local,
                                Name = "SustainedSpellsRecalculateCosts",
                                Properties = new()
                                {
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsDrainList",
                                        Object = drainSpellsListAlteration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsBaseList",
                                        Object = baseSpellsListAlteration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsEffectList",
                                        Object = lastingSpellsListAlteration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsTrackerList",
                                        Object = lastingSpellTrackerListAlteration.ToLink()
                                    }
                                }
                            });
                            state.PatchMod.MagicEffects.Set(magicEffect);
                            break;
                        case ActorValue.DestructionModifier:
                            magicEffect.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                            {
                                Flags = ScriptEntry.Flag.Local,
                                Name = "SustainedSpellsRecalculateCosts",
                                Properties = new()
                                {
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsDrainList",
                                        Object = drainSpellsListDestruction.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsBaseList",
                                        Object = baseSpellsListDestruction.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsEffectList",
                                        Object = lastingSpellsListDestruction.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsTrackerList",
                                        Object = lastingSpellTrackerListDestruction.ToLink()
                                    }
                                }
                            });
                            state.PatchMod.MagicEffects.Set(magicEffect);
                            break;
                        case ActorValue.RestorationModifier:
                            magicEffect.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                            {
                                Flags = ScriptEntry.Flag.Local,
                                Name = "SustainedSpellsRecalculateCosts",
                                Properties = new()
                                {
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsDrainList",
                                        Object = drainSpellsListRestoration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsBaseList",
                                        Object = baseSpellsListRestoration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsEffectList",
                                        Object = lastingSpellsListRestoration.ToLink()
                                    },
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SustainedSpellsTrackerList",
                                        Object = lastingSpellTrackerListRestoration.ToLink()
                                    }
                                }
                            });
                            state.PatchMod.MagicEffects.Set(magicEffect);
                            break;
                    }
                }
            }

            var drainEffectFlags = new MagicEffect.Flag();
            drainEffectFlags |= MagicEffect.Flag.Recover;
            drainEffectFlags |= MagicEffect.Flag.Detrimental;
            drainEffectFlags |= MagicEffect.Flag.NoHitEffect;
            drainEffectFlags |= MagicEffect.Flag.NoDuration;
            drainEffectFlags |= MagicEffect.Flag.NoArea;
            drainEffectFlags |= MagicEffect.Flag.Painless;

            var descriptionEffectFlags = new MagicEffect.Flag();
            descriptionEffectFlags |= MagicEffect.Flag.NoHitEffect;
            descriptionEffectFlags |= MagicEffect.Flag.NoHitEvent;
            descriptionEffectFlags |= MagicEffect.Flag.NoDuration;
            descriptionEffectFlags |= MagicEffect.Flag.NoArea;
            descriptionEffectFlags |= MagicEffect.Flag.NoMagnitude;

            foreach (var bookGetter in state.LoadOrder.PriorityOrder.Book().WinningOverrides())
            {
                if (bookGetter.Teaches is BookSpell spellGetter)
                {
                    var spell = spellGetter.Spell.Resolve(state.LinkCache);
                    if (blacklist.Contains(spell.ToLinkGetter())) continue;
                    var firstSpellEffect = spell.Effects[0];
                    if (firstSpellEffect.Data?.Duration == 0) continue;
                    var firstMagicEffect = firstSpellEffect.BaseEffect.Resolve(state.LinkCache);
                    SpellArchetypeType spellType = GetSpellType(firstMagicEffect);
                    ActorValue spellSkill = firstMagicEffect.MagicSkill;
                    if (IsSpellTypeEnabled(spellType))
                    {
                        Console.WriteLine(spell.EditorID);
                        Spell lastingSpell;
                        if (Settings.Value.replaceSpells)
                        {
                            lastingSpell = spell.DeepCopy();
                        }
                        else
                        {
                            lastingSpell = spell.Duplicate(state.PatchMod.GetNextFormKey());
                            lastingSpell.EditorID = "SustainedSpell_" + lastingSpell.EditorID;
                            lastingSpell.Name = "Lasting " + lastingSpell;
                        }
                        lastingSpell.Effects.Clear();
                        if (!firstMagicEffect.Flags.HasFlag(MagicEffect.Flag.PowerAffectsMagnitude))
                        {
                            lastingSpell.Flags |= SpellDataFlag.NoDualCastModification;
                        }

                        MiscItem tracker = new(state.PatchMod, "SustainedSpellTracker_" + lastingSpell.EditorID)
                        {
                            MajorFlags = MiscItem.MajorFlag.NonPlayable
                        };
                        state.PatchMod.MiscItems.Set(tracker);

                        string lastingSpellDescription = " Caster will have reduced magicka while this spell is active.";
                        float longestChargeTime = 0;

                        MagicEffect drainEffect = new(state.PatchMod)
                        {
                            EditorID = "SustainedSpellDrainEffect_" + spell.EditorID,
                            Archetype = new MagicEffectArchetype()
                            {
                                ActorValue = ActorValue.Magicka,
                                Type = MagicEffectArchetype.TypeEnum.ValueModifier
                            },
                            MenuDisplayObject = spell.MenuDisplayObject.AsNullable(),
                            CastType = CastType.ConstantEffect,
                            CastingSoundLevel = SoundLevel.Silent,
                            Flags = drainEffectFlags,
                            Name = "Reduced Magicka",
                            Description = lastingSpellDescription
                        };
                        Spell drainSpell = new(state.PatchMod)
                        {
                            EditorID = "SustainedSpellDrain_" + spell.EditorID,
                            Type = SpellType.Ability,
                            CastType = CastType.ConstantEffect,
                            TargetType = TargetType.Self,
                            Flags = SpellDataFlag.NoAbsorbOrReflect,
                            Name = lastingSpell.Name,
                            Effects = new()
                            {
                                new Effect()
                                {
                                    BaseEffect = drainEffect.ToNullableLink<IMagicEffectGetter>(),
                                    Data = new()
                                }
                            }
                        };
                        state.PatchMod.Spells.Set(drainSpell);

                        switch (spellSkill)
                        {
                            case ActorValue.Conjuration:
                                lastingSpellsListConjuration.Items.Add(lastingSpell);
                                baseSpellsListConjuration.Items.Add(spell);
                                lastingSpellTrackerListConjuration.Items.Add(tracker);
                                drainSpellsListConjuration.Items.Add(drainSpell);
                                break;
                            case ActorValue.Restoration:
                                lastingSpellsListRestoration.Items.Add(lastingSpell);
                                baseSpellsListRestoration.Items.Add(spell);
                                lastingSpellTrackerListRestoration.Items.Add(tracker);
                                drainSpellsListRestoration.Items.Add(drainSpell);
                                break;
                            case ActorValue.Alteration:
                                lastingSpellsListAlteration.Items.Add(lastingSpell);
                                baseSpellsListAlteration.Items.Add(spell);
                                lastingSpellTrackerListAlteration.Items.Add(tracker);
                                drainSpellsListAlteration.Items.Add(drainSpell);
                                break;
                            case ActorValue.Destruction:
                                lastingSpellsListDestruction.Items.Add(lastingSpell);
                                baseSpellsListDestruction.Items.Add(spell);
                                lastingSpellTrackerListDestruction.Items.Add(tracker);
                                drainSpellsListDestruction.Items.Add(drainSpell);
                                break;
                            case ActorValue.Illusion:
                                lastingSpellsListIllusion.Items.Add(lastingSpell);
                                baseSpellsListIllusion.Items.Add(spell);
                                lastingSpellTrackerListIllusion.Items.Add(tracker);
                                drainSpellsListIllusion.Items.Add(drainSpell);
                                break;
                        }

                        foreach (var spellEffect in spell.Effects)
                        {
                            var lastingSpellEffect = spellEffect.DeepCopy();
                            lastingSpellEffect.Data ??= new();
                            lastingSpellEffect.Data.Duration = 86400;

                            var magicEffect = spellEffect.BaseEffect.Resolve(state.LinkCache);
                            if (magicEffect.SpellmakingCastingTime > longestChargeTime) longestChargeTime = magicEffect.SpellmakingCastingTime;
                            if (!magicEffect.Flags.HasFlag(MagicEffect.Flag.HideInUI))
                            {
                                var description = magicEffect.Description?.ToString();
                                if (description != null && description.Length > 0)
                                {
                                    if (description.StartsWith("For <dur> seconds, "))
                                    {
                                        lastingSpellDescription = CapatalizeFirst(description.Replace("For <dur> seconds, ", "")).Replace("<mag>", "<" + spellEffect.Data?.Magnitude.ToInt().ToString() + ">") + lastingSpellDescription;
                                    }
                                    else
                                    {
                                        lastingSpellDescription = description.Replace(" for <dur> seconds", "").Replace("<mag>", "<" + spellEffect.Data?.Magnitude.ToInt().ToString() + ">") + lastingSpellDescription;
                                    }
                                }
                            }
                            if (spellEffect.Data != null && spellEffect.Data.Duration > 0)
                            {
                                var lastingMagicEffect = magicEffect.Duplicate(state.PatchMod.GetNextFormKey());
                                lastingSpellEffect.BaseEffect.SetTo(lastingMagicEffect);
                                lastingMagicEffect.EditorID = "SustainedSpellEffect_" + lastingMagicEffect.EditorID;
                                lastingMagicEffect.Flags |= MagicEffect.Flag.HideInUI;
                                lastingMagicEffect.Keywords ??= new();
                                if (spellType == SpellArchetypeType.Summon)
                                {
                                    lastingMagicEffect.Keywords.Add(FormKey.Factory("000800:Sustained Spells.esp"));
                                }
                                else if(spellType == SpellArchetypeType.Reanimate)
                                {
                                    lastingMagicEffect.Keywords.Add(FormKey.Factory("000D6D:Sustained Spells.esp"));
                                }
                                else if (spellType == SpellArchetypeType.BoundWeapon)
                                {
                                    lastingMagicEffect.Keywords.Add(FormKey.Factory("000D6E:Sustained Spells.esp"));
                                }
                                else if (spellType == SpellArchetypeType.Cloak)
                                {
                                    lastingMagicEffect.Keywords.Add(FormKey.Factory("000D66:Sustained Spells.esp"));
                                }
                                else if (spellType == SpellArchetypeType.SoulCloak)
                                {
                                    lastingMagicEffect.Keywords.Add(FormKey.Factory("000D67:Sustained Spells.esp"));
                                }
                                else if (spellType == SpellArchetypeType.Waterbreathing)
                                {
                                    lastingMagicEffect.Keywords.Add(FormKey.Factory("000D6F:Sustained Spells.esp"));
                                }
                                else if (spellType == SpellArchetypeType.Feather)
                                {
                                    lastingMagicEffect.Keywords.Add(FormKey.Factory("000D70:Sustained Spells.esp"));
                                }
                                else if (spellType == SpellArchetypeType.Flesh)
                                {
                                    lastingMagicEffect.Keywords.Add(FormKey.Factory("000D71:Sustained Spells.esp"));
                                }
                                else if (spellType == SpellArchetypeType.Shield)
                                {
                                    lastingMagicEffect.Keywords.Add(FormKey.Factory("000D72:Sustained Spells.esp"));
                                }
                                else if (spellType == SpellArchetypeType.Candlelight)
                                {
                                    lastingMagicEffect.Keywords.Add(FormKey.Factory("000D73:Sustained Spells.esp"));
                                }
                                else if (spellType == SpellArchetypeType.Muffle)
                                {
                                    lastingMagicEffect.Keywords.Add(FormKey.Factory("000D73:Sustained Spells.esp"));
                                }

                                if (spellType == GetSpellType(lastingMagicEffect) && lastingMagicEffect.Archetype.ActorValue == firstMagicEffect.Archetype.ActorValue)
                                {
                                    lastingMagicEffect.VirtualMachineAdapter ??= new();
                                    lastingMagicEffect.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                                    {

                                        Flags = ScriptEntry.Flag.Local,
                                        Name = "SustainedSpell",
                                        Properties = new()
                                        {
                                            new ScriptObjectProperty()
                                            {
                                                Flags = ScriptProperty.Flag.Edited,
                                                Name = "basespell",
                                                Object = spell.ToLink()
                                            },
                                            new ScriptObjectProperty()
                                            {
                                                Flags = ScriptProperty.Flag.Edited,
                                                Name = "togglespellnegative",
                                                Object = drainSpell.ToLink()
                                            },
                                            new ScriptObjectProperty()
                                            {
                                                Flags = ScriptProperty.Flag.Edited,
                                                Name = "togglespelleffect",
                                                Object = lastingMagicEffect.ToLink()
                                            },
                                            new ScriptObjectProperty()
                                            {
                                                Flags = ScriptProperty.Flag.Edited,
                                                Name = "spelltracker",
                                                Object = tracker.ToLink()
                                            },
                                            new ScriptIntProperty()
                                            {
                                                Flags = ScriptProperty.Flag.Edited,
                                                Name = "Skill",
                                                Data = (int)spellSkill
                                            }
                                        }
                                    });
                                }
                                state.PatchMod.MagicEffects.Set(lastingMagicEffect);
                            }
                            lastingSpell.Effects.Add(lastingSpellEffect);
                        }

                        var lastingSpellDescriptionEffect = new MagicEffect(state.PatchMod, "SustainedSpellDescription_" + spell.EditorID)
                        {
                            Description = lastingSpellDescription,
                            Archetype = new MagicEffectArchetype()
                            {
                                Type = MagicEffectArchetype.TypeEnum.Script,
                                ActorValue = ActorValue.None
                            },
                            CastingSoundLevel = SoundLevel.Silent,
                            CastType = CastType.FireAndForget,
                            TargetType = TargetType.TargetLocation,
                            Flags = descriptionEffectFlags
                        };
                        lastingSpell.Effects.Add(new Effect()
                        {
                            BaseEffect = lastingSpellDescriptionEffect.ToNullableLink<IMagicEffectGetter>(),
                            Data = new()
                        });
                        drainEffect.Description = lastingSpellDescription + " (<mag> Magicka)";


                        if (!lastingSpell.Flags.HasFlag(SpellDataFlag.ManualCostCalc))
                        {
                            lastingSpell.Flags |= SpellDataFlag.ManualCostCalc;
                            lastingSpell.ChargeTime = longestChargeTime;
                        }
                        state.PatchMod.Spells.Set(lastingSpell);
                        state.PatchMod.MagicEffects.Set(drainEffect);
                        state.PatchMod.MagicEffects.Set(lastingSpellDescriptionEffect);

                        if (!Settings.Value.replaceSpells)
                        {
                            var book = bookGetter.DeepCopy();
                            book.VirtualMachineAdapter ??= new();
                            book.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                            {
                                Flags = ScriptEntry.Flag.Local,
                                Name = "LearnSpellonRead",
                                Properties = new()
                                {
                                    new ScriptObjectProperty()
                                    {
                                        Flags = ScriptProperty.Flag.Edited,
                                        Name = "SpellLearned",
                                        Object = lastingSpell.ToLink()
                                    }
                                }
                            });
                            state.PatchMod.Books.Set(book);
                        }
                    }
                }
            }

            state.PatchMod.FormLists.Set(drainSpellsListConjuration);
            state.PatchMod.FormLists.Set(baseSpellsListConjuration);
            state.PatchMod.FormLists.Set(lastingSpellsListConjuration);
            state.PatchMod.FormLists.Set(lastingSpellTrackerListConjuration);
            state.PatchMod.FormLists.Set(drainSpellsListAlteration);
            state.PatchMod.FormLists.Set(baseSpellsListAlteration);
            state.PatchMod.FormLists.Set(lastingSpellsListAlteration);
            state.PatchMod.FormLists.Set(lastingSpellTrackerListAlteration);
            state.PatchMod.FormLists.Set(drainSpellsListRestoration);
            state.PatchMod.FormLists.Set(baseSpellsListRestoration);
            state.PatchMod.FormLists.Set(lastingSpellsListRestoration);
            state.PatchMod.FormLists.Set(lastingSpellTrackerListRestoration);
            state.PatchMod.FormLists.Set(drainSpellsListDestruction);
            state.PatchMod.FormLists.Set(baseSpellsListDestruction);
            state.PatchMod.FormLists.Set(lastingSpellsListDestruction);
            state.PatchMod.FormLists.Set(lastingSpellTrackerListDestruction);
            state.PatchMod.FormLists.Set(drainSpellsListIllusion);
            state.PatchMod.FormLists.Set(baseSpellsListIllusion);
            state.PatchMod.FormLists.Set(lastingSpellsListIllusion);
            state.PatchMod.FormLists.Set(lastingSpellTrackerListIllusion);
        }
    }
}