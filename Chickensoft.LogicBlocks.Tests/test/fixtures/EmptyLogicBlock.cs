namespace Chickensoft.LogicBlocks.Tests.Fixtures;

using Chickensoft.Introspection;

[Meta("empty_logic_block")]
[LogicBlock(typeof(State), Diagram = false)]
public partial class EmptyLogicBlock : LogicBlock<EmptyLogicBlock.State> {
  public override Transition GetInitialState() => To<State>();

  [Meta("empty_logic_block_state")]
  public partial record State : StateLogic<State>;
}
