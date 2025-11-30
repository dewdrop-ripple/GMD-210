using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;

namespace BehaviorTreeLib {
    public interface IStrategy {
        Node.Status Process();
        void Reset() {
            // Noop
        }
    }

    public class UntilFail : Node {
        public UntilFail(string name) : base(name) { }

        public override Status Process() {
            if (children[0].Process() == Status.FAILURE) {
                Reset();
                return Status.FAILURE;
            }
            return Status.RUNNING;
        }
    }

    public class Inverter : Node {
        public Inverter(string name) : base(name) { }

        public override Status Process() {
            switch (children[0].Process()) {
                case Status.RUNNING:
                    return Status.RUNNING;
                case Status.FAILURE:
                    return Status.SUCCESS;
                default:
                    return Status.FAILURE;
            }
        }
    }

    public class PrioritySelector : Selector {
        List<Node> sortedChildren;
        List<Node> SortedChildren => sortedChildren ??= SortChildren();

        protected virtual List<Node> SortChildren() => children.OrderByDescending(child => child.priority).ToList();

        public PrioritySelector(string name) : base(name) { }

        public override void Reset() {
            base.Reset();
            sortedChildren = null;
        }

        public override Status Process() {
            foreach (var child in SortedChildren) {
                switch (child.Process()) {
                    case Status.RUNNING:
                        return Status.RUNNING;
                    case Status.SUCCESS: 
                        return Status.SUCCESS;
                    default:
                        continue;

                }
            }

            return Status.FAILURE;
        }
    }

    public class Selector : Node {
        public Selector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process() { 
            if (currentChild < children.Count) {
                switch (children[currentChild].Process()) {
                    case Status.RUNNING:
                        return Status.RUNNING;
                    case Status.SUCCESS:
                        return Status.SUCCESS;
                    default:
                        currentChild++;
                        return Status.RUNNING;
                }
            }

            Reset();
            return Status.FAILURE;
        }
    }

    public class Sequence : Node {
        public Sequence(string name, int priority = 0) : base(name, priority) { }

        public override Status Process() {
            {
                if (currentChild < children.Count) {
                    switch (children[currentChild].Process()) {
                        case Status.RUNNING:
                            return Status.RUNNING;
                        case Status.FAILURE:
                            Reset();
                            return Status.FAILURE;
                        default:
                            currentChild++;
                            return currentChild == children.Count ? Status.SUCCESS : Status.RUNNING;
                    }
                }

                Reset();
                return Status.SUCCESS;
            }
        }
    }

    public class Leaf : Node {
        readonly IStrategy strategy;

        public Leaf(string name, IStrategy strategy, int priority = 0): base(name, priority) {
            this.strategy = strategy;
        }

        public override Status Process() => strategy.Process();

        public override void Reset() => strategy.Reset();
    }

    public class Node {
        public enum Status {
            SUCCESS,
            FAILURE,
            RUNNING
        }

        public readonly string name;
        public readonly int priority;

        public readonly List<Node> children = new();
        protected int currentChild;

        public Node(string name = "Node", int priority = 0) {
            this.name = name;
            this.priority = priority;
        }

        public void AddChild(Node child) => children.Add(child);

        public virtual Status Process() => children[currentChild].Process();

        public virtual void Reset() {
            currentChild = 0;
            foreach (var child in children) {
                child.Reset();
            }
        }
    }

    public class BehaviorTree : Node {
        public BehaviorTree(string name) : base(name) { }

        public override Status Process() {
            while (currentChild < children.Count) {
                var status = children[currentChild].Process();
                if (status != Status.SUCCESS) {
                    return status;
                }
                currentChild++;
            }
            return Status.SUCCESS;
        }
    }

    public class Condition : IStrategy {
        readonly Func<bool> predicate;

        public Condition(Func<bool> predicate) {
            this.predicate = predicate;
        }

        public Node.Status Process() {
            bool result = predicate();
            if (result) {
                return Node.Status.SUCCESS;
            } else {
                return Node.Status.FAILURE;
            }
        }
    }

    public class ActionStrategy : IStrategy {
        readonly Action doSomething;

        public ActionStrategy(Action doSomething) {
            this.doSomething = doSomething;
        }

        public Node.Status Process() {
            doSomething();
            return Node.Status.SUCCESS;
        }
    }

    public class Move : IStrategy {
        readonly Action<Vector2> moveToward;
        readonly Func<Vector2> getLocation;
        Vector2 location;
        readonly Transform npc;

        public Move(Action<Vector2> moveToward, Func<Vector2> getLocation, Transform npc) {
            this.moveToward = moveToward;
            this.getLocation = getLocation;
            this.npc = npc;
        }

        public Node.Status Process() {
            location = getLocation();
            if (Vector2.Distance(location, npc.position) < 0.1f) {
                return Node.Status.SUCCESS;
            }
                moveToward(location);
            if (Vector2.Distance(location, npc.position) > 0.1f) {
                return Node.Status.RUNNING;
            }
            return Node.Status.SUCCESS;
        }
    }

    public class InteractWithObject : IStrategy {
        readonly Func<bool> interact;
        public InteractWithObject(Func<bool> interact) {
            this.interact = interact;
        }

        public Node.Status Process() {
            if (!interact()) {
                return Node.Status.RUNNING;
            }
            return Node.Status.SUCCESS;
        }
    }
}