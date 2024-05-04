namespace Chickensoft.LogicBlocks.Tests.Fixtures;

using System;
using System.Collections.Generic;
using Chickensoft.Introspection;

[Meta("fake_logic_block")]
public partial class FakeLogicBlock {
  public static class Input {
    public readonly record struct InputOne(int Value1, int Value2);
    public readonly record struct InputTwo(string Value1, string Value2);
    public readonly record struct InputThree(string Value1, string Value2);
    public readonly record struct InputError;
    public readonly record struct InputUnknown;
    public readonly record struct GetString;
    public readonly record struct NoNewState;
    public readonly record struct SelfInput(InputOne Input);
    public readonly record struct InputCallback(
      Action Callback,
      Func<IContext, Transition> Next
    );
    public readonly record struct Custom(Func<IContext, Transition> Next);
  }

  [Meta("fake_logic_block_state")]
  public abstract partial record State : StateLogic<State>,
    IGet<Input.InputOne>,
    IGet<Input.InputTwo>,
    IGet<Input.InputThree>,
    IGet<Input.InputError>,
    IGet<Input.NoNewState>,
    IGet<Input.InputCallback>,
    IGet<Input.GetString>,
    IGet<Input.SelfInput>,
    IGet<Input.Custom> {
    public Transition On(Input.InputOne input) {
      Output(new Output.OutputOne(1));
      return To<StateA>().With(state => {
        var a = (StateA)state;
        a.Value1 = input.Value1;
        a.Value2 = input.Value2;
      });
    }

    public Transition On(Input.InputTwo input) {
      Output(new Output.OutputTwo("2"));
      return To<StateB>().With(state => {
        var b = (StateB)state;
        b.Value1 = input.Value1;
        b.Value2 = input.Value2;
      });
    }

    public Transition On(Input.InputThree input) => To<StateD>().With(state => {
      var d = (StateD)state;
      d.Value1 = input.Value1;
      d.Value2 = input.Value2;
    });

    public Transition On(Input.InputError input)
      => throw new InvalidOperationException();

    public Transition On(Input.NoNewState input) {
      Output(new Output.OutputOne(1));
      return ToSelf();
    }

    public Transition On(Input.InputCallback input) {
      input.Callback();
      return input.Next(Context);
    }

    public Transition On(Input.Custom input) => input.Next(Context);

    public Transition On(Input.GetString input) => To<StateC>()
      .With(state => ((StateC)state).Value = Get<string>());

    public Transition On(Input.SelfInput input) {
      Input(input.Input);
      return ToSelf();
    }

    [Meta("fake_logic_block_state_start")]
    public partial record StartState : State;

    [Meta("fake_logic_block_state_a")]
    public partial record StateA : State {
      public int Value1 { get; set; }
      public int Value2 { get; set; }
    }

    [Meta("fake_logic_block_state_b")]
    public partial record StateB : State {
      public string Value1 { get; set; } = default!;
      public string Value2 { get; set; } = default!;
    }

    [Meta("fake_logic_block_state_c")]
    public partial record StateC : State {
      public string Value { get; set; } = default!;
    }

    [Meta("fake_logic_block_state_d")]
    public partial record StateD : State {
      public string Value1 { get; set; } = default!;
      public string Value2 { get; set; } = default!;
    }

    [Meta("fake_logic_block_state_nothing")]
    public partial record NothingState : State;

    [Meta("fake_logic_block_state_on_enter")]
    public partial record OnEnterState : State {
      public Action<State?> Callback { get; set; } = default!;

      public OnEnterState() {
        this.OnEnter((State? previous) => Callback(previous));
      }
    }

    [Meta("fake_logic_block_state_on_exit")]
    public partial record OnExitState : State {
      public Action<State?> Callback { get; set; } = default!;

      public OnExitState() {
        this.OnExit((State? next) => Callback(next));
      }
    }

    [Meta("fake_logic_block_state_add_error")]
    public partial record AddErrorOnEnterState : State {
      public Exception E { get; set; } = default!;

      public AddErrorOnEnterState() {
        this.OnEnter(() => AddError(E));
      }
    }
  }

  public static class Output {
    public readonly record struct OutputOne(int Value);
    public readonly record struct OutputTwo(string Value);
  }
}

[LogicBlock(typeof(State), Diagram = true)]
public partial class FakeLogicBlock : LogicBlock<FakeLogicBlock.State> {
  public Func<Transition>? InitialState { get; set; }

  public List<Exception> Exceptions { get; } = new();

  public override Transition GetInitialState() =>
    InitialState?.Invoke() ?? To<State.StartState>();

  private readonly Action<Exception>? _onError;

  public FakeLogicBlock(Action<Exception>? onError = null) {
    _onError = onError;
  }

  ~FakeLogicBlock() { }

  protected override void HandleError(Exception e) {
    Exceptions.Add(e);
    _onError?.Invoke(e);
    base.HandleError(e);
  }
}
