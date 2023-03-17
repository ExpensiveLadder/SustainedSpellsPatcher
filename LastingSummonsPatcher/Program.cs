using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;
using DynamicData;
using Noggog;
using Mutagen.Bethesda.WPF.Reflection.Attributes;

namespace SustainedSpellsPatcher
{
    public class TestSettings
    {
        //public double InitialCostMultiplier = 0.0;

        //public double LastingCostMultiplier = 1.0;

        public bool replaceSpells = true;

        [SettingName("Blacklisted FormKeys")]
        public List<string> blacklist = new();

        //public int maxSummons = 2;;
    }

    public class Program
    {
        static Lazy<TestSettings> Settings = null!;

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

            List<string> blacklist = Settings.Value.blacklist;
            if (!state.LoadOrder.ModExists(new ModKey("MysticismMagic", ModType.Plugin), true))
            {
                blacklist.Add("07E5D5:Skyrim.esm"); // Flame Thrall
                blacklist.Add("07E5D6:Skyrim.esm"); // Frost Thrall
                blacklist.Add("07E5D7:Skyrim.esm"); // Storm Thrall
            }
            blacklist.Add("07E8DF:Skyrim.esm"); // Dead Thrall

            FormList drainSpellsListConjuration = new(state.PatchMod, "SustainedSpellsDrainListConjuration");
            FormList baseSpellsListConjuration = new(state.PatchMod, "SustainedSpellsBaseListConjuration");
            FormList lastingSpellsListConjuration = new(state.PatchMod, "SustainedSpellsEffectListConjuration");
            FormList lastingSpellTrackerListConjuration = new(state.PatchMod, "SustainedSpellsTrackerListConjuration");

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
                                        Object = FormKey.Factory("000802:Sustained Spells.esp").ToLink<Spell>()
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
                                        Object = FormKey.Factory("000802:Sustained Spells.esp").ToLink<Spell>()
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
                                        Name = "SustainedSpellsEffectList",
                                        Object = lastingSpellTrackerListConjuration.ToLink()
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
                                        Name = "SustainedSpellsEffectList",
                                        Object = lastingSpellTrackerListConjuration.ToLink()
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
                    if (blacklist.Contains(spell.FormKey.ToString())) continue;
                    var firstEffect = spell.Effects[0].BaseEffect.Resolve(state.LinkCache);
                    if (firstEffect.Archetype.Type == MagicEffectArchetype.TypeEnum.SummonCreature || firstEffect.Archetype.Type == MagicEffectArchetype.TypeEnum.Reanimate)
                    {
                        Console.WriteLine(spell.EditorID);
                        Spell lastingSpell;
                        if (Settings.Value.replaceSpells)
                        {
                            lastingSpell = spell.DeepCopy();
                        } else
                        {
                            lastingSpell = spell.Duplicate(state.PatchMod.GetNextFormKey());
                            lastingSpell.EditorID = "SustainedSpell_" + lastingSpell.EditorID;
                        }
                        lastingSpell.Effects.Clear();
                        lastingSpell.Flags |= SpellDataFlag.NoDualCastModification;
                        if (!Settings.Value.replaceSpells) lastingSpell.Name = "Lasting " + lastingSpell;

                        lastingSpellsListConjuration.Items.Add(lastingSpell);
                        baseSpellsListConjuration.Items.Add(spell);

                        MiscItem tracker = new(state.PatchMod, "SustainedSpellTracker_" + lastingSpell.EditorID)
                        {
                            MajorFlags = MiscItem.MajorFlag.NonPlayable
                        };
                        state.PatchMod.MiscItems.Set(tracker);
                        lastingSpellTrackerListConjuration.Items.Add(tracker);

                        string lastingSpellDescription = " Caster will have reduced magicka while this spell is active.";
                        float longestChargeTime = 0;

                        MagicEffect drainEffect = new(state.PatchMod)
                        {
                            EditorID = "SustainedSpellDrainEffect_" + spell.EditorID,
                            Archetype = new MagicEffectArchetype() {
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
                                },
                                new Effect()
                                {
                                    BaseEffect = drainEffect.ToNullableLink<IMagicEffectGetter>(),
                                    Data = new(),
                                    Conditions = new ExtendedList<Condition>()
                                    {
                                        new ConditionFloat()
                                        {
                                            CompareOperator = CompareOperator.GreaterThanOrEqualTo,
                                            ComparisonValue = 2,
                                            Data = new FunctionConditionData()
                                            {
                                                RunOnType = Condition.RunOnType.Subject,
                                                Function = Condition.Function.GetItemCount,
                                                ParameterOneRecord = tracker.ToLink()
                                            }
                                        }
                                    }
                                }
                            }
                        };
                        state.PatchMod.Spells.Set(drainSpell);
                        drainSpellsListConjuration.Items.Add(drainSpell);

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
                                    lastingSpellDescription = description.Replace(" for <dur> seconds", "") + lastingSpellDescription;
                                }
                            }
                            if (magicEffect.Archetype.Type == MagicEffectArchetype.TypeEnum.SummonCreature || magicEffect.Archetype.Type == MagicEffectArchetype.TypeEnum.Reanimate)
                            {
                                var lastingMagicEffect = magicEffect.Duplicate(state.PatchMod.GetNextFormKey());
                                lastingSpellEffect.BaseEffect.SetTo(lastingMagicEffect);
                                lastingMagicEffect.EditorID = "SustainedSpellEffect_" + lastingMagicEffect.EditorID;
                                lastingMagicEffect.Flags |= MagicEffect.Flag.HideInUI;
                                lastingMagicEffect.Keywords ??= new();
                                lastingMagicEffect.Keywords.Add(FormKey.Factory("000800:Sustained Spells.esp"));
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
                                            Data = 19
                                        }
                                        /*
                                        new ScriptObjectProperty()
                                        {
                                            Flags = ScriptProperty.Flag.Edited,
                                            Name = "PlayerRef",
                                            Object = FormKey.Factory("000014:Skyrim.esm").ToLink()
                                        }
                                        */
                                    }
                                });
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
        }
    }
}