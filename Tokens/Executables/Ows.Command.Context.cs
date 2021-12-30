﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public static partial class Ows {

    public partial class Command {

      /// <summary>
      /// Context used for executing a command
      /// </summary>
      public struct Context : IEnumerable<IParameter> {

        /// <summary>
        /// The executed command
        /// </summary>
        public readonly Command Command;

        /// <summary>
        /// The character who executed the command
        /// </summary>
        public readonly Data.Character Executor;


        internal readonly IList<IParameter> _extraParameters;
        internal readonly VariableMap _temporaryScopedVariables;
        internal readonly Index _indexReplacement;
        List<IParameter> _compiledParameters;

        /// <summary>
        /// The ordered parameters passed to this command
        /// </summary>
        public readonly IReadOnlyList<IParameter> OrderedParameters
          => _compiledParameters;

        /// <summary>
        /// The temp scoped params currently available to this command
        /// </summary>
        public readonly IReadOnlyDictionary<string, Variable> ScopedParams
          => _temporaryScopedVariables.Value;

        internal Context(
          Command command,
          Data.Character executor,
          IList<IParameter> extraParameters = null,
          VariableMap scopedParameters = null,
          Index indexReplacement = null
        ) {
          Command = command;
          Executor = executor;
          _extraParameters = extraParameters;
          _temporaryScopedVariables = scopedParameters;
          _indexReplacement = indexReplacement;
          _compiledParameters = Command.Parameters?.Concat(_extraParameters ?? Enumerable.Empty<IParameter>())
            .Select(param =>
            param.Value is PlaceholderIndex index
              ? indexReplacement is null
                ? throw new ArgumentException($"Index provided as an argument but no replacement provided to the execute for function")
                : (IParameter)indexReplacement
              : param.Value is CharacterSpecificVariable characterSpecific
                ? (IParameter)characterSpecific.GetFor(executor)
                : (IParameter)param.Value)?.ToList();
        }

        internal Context _addExtraParameter(IParameter parameter) {
          _extraParameters.Add(parameter);
          _compiledParameters.Add(parameter);
          return this;
        }

        public IParameter GetParameter(int index)
          => _compiledParameters[index];

        public Variable GetUltimateParameterVariable(int index)
          => _compiledParameters[index].GetUltimateVariableFor(this);

        public Variable GetVariable(string name)
          => _temporaryScopedVariables.Value.TryGetValue(name, out var local)
            ? local
            : Command.Program.GetVariableByName(name);

        public Variable GetTempScopedVariable(string name)
          => _temporaryScopedVariables.Value[name];

        public Variable GetGlobalProgramVariable(string name)
          => Command.Program._globals[name];

        public Variable GetGlobalWorldVariable(string name)
          => Ows._globals[name] as Variable;

        public Variable GetLocalVariableFor(string characterId, string name)
          => Command.Program.GetVariableByName(characterId, name);

        public Variable GetGlobalVariableFor(string characterId, string name)
          => Ows._globalVariablesByCharacter[characterId][name];

        public TParameter GetParameter<TParameter>(int index)
          where TParameter : IParameter
            => (TParameter)_compiledParameters[index];

        public TVariable GetUltimateParameterVariable<TVariable>(int index)
          where TVariable : Variable
          => _compiledParameters[index].GetUltimateVariableAs<TVariable>(this);

        public TVariable GetVariable<TVariable>(string name)
          where TVariable : Variable
            => (TVariable)(_temporaryScopedVariables.Value.TryGetValue(name, out var local)
              ? local
              : Command.Program.GetVariableByName(name));

        public TVariable TryToGetVariableByName<TVariable>(string name)
          where TVariable : Variable
            => (TVariable)(_temporaryScopedVariables.Value.TryGetValue(name, out var local)
              ? local
              : Command.Program.TryToGetVariableByName(name));

        public TVariable GetTempScopedVariable<TVariable>(string name)
          where TVariable : Variable
            => (TVariable)_temporaryScopedVariables.Value[name];

        public TVariable GetGlobalProgramVariable<TVariable>(string name)
          where TVariable : Variable
            => (TVariable)Command.Program._globals[name];

        public TVariable GetGlobalWorldVariable<TVariable>(string name)
          where TVariable : Variable
            => (TVariable)(Ows._globals[name] as Variable);

        public TVariable GetLocalVariableFor<TVariable>(string characterId, string name)
          where TVariable : Variable
            => (TVariable)Command.Program.GetVariableByName(characterId, name);

        public TVariable GetGlobalVariableFor<TVariable>(string characterId, string name)
          where TVariable : Variable
            => (TVariable)Ows._globalVariablesByCharacter[characterId][name];

        public IEnumerator<IParameter> GetEnumerator()
          => (_compiledParameters ?? Enumerable.Empty<IParameter>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
          => GetEnumerator();
      }
    }
  }

  public static class ContextExtensions {

    /// <summary>
    /// Add an extra parameter to the context
    /// </summary>
    public static Ows.Command.Context AddExtraParameter(this Ows.Command.Context context, Ows.IParameter parameter)
      => context._addExtraParameter(parameter);
  }
}