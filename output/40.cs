using Framework.GalaSports.Service;
using GraphProcessor;
using NodeGraphProcessor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NodeGraphProcessor
{
    public class NoviceProcedure : BaseGraphProcessor
    {
        List<BaseNode> processList;
        List<StartNode> startNodeList;
        List<WaitableNode> currentWaitables = new List<WaitableNode> ();
        Dictionary<BaseNode, List<BaseNode>> nonConditionalDependenciesCache = new Dictionary<BaseNode, List<BaseNode>>();

        public IEnumerator<BaseNode> currentGraphExecution { get; private set; } = null;

        public NoviceProcedure(BaseGraph graph) : base(graph) { }

        public override void UpdateComputeOrder()
        {
            startNodeList = graph.nodes.Where(n => n is StartNode).Select(n => n as StartNode).ToList();

            if (startNodeList.Count == 0)
            {
                processList = graph.nodes.OrderBy(n => n.computeOrder).ToList();
            }
            else
            {
                nonConditionalDependenciesCache.Clear();
            }
        }

        public override void Run()
        {
            if (currentWaitables == null)
                currentWaitables = new List<WaitableNode>();
            else
                this.Destroy();

            IEnumerator<BaseNode> enumerator;

            if (startNodeList.Count == 0)
            {
                enumerator = RunTheGraph();
            }
            else
            {
                Stack<BaseNode> nodeToExecute = new Stack<BaseNode>();
                startNodeList.ForEach(s => nodeToExecute.Push(s));
                enumerator = RunTheGraph(nodeToExecute);
            }

            while (enumerator.MoveNext())
                 

        }

        public override void Run(Stack<BaseNode> baseNode)
        {
            if (currentWaitables == null)
                currentWaitables = new List<WaitableNode>();
            else
                Destroy();
            WaitedRun(baseNode);
        }

        public override void Destroy()
        {
            for (int i = 0; i < currentWaitables.Count; i++)
            {
                
            }
            currentWaitables.Clear();
        }

        private void WaitedRun(Stack<BaseNode> nodes)
        {
            if (currentWaitables!= null && currentWaitables.Count == 0)
            {
                Platform.EventDispatcher.TriggerEvent("PROCEDURE_END", this.graph.name);
            }
        }

        IEnumerable<BaseNode> GatherNonConditionalDependencies(BaseNode node)
        {
            Stack<BaseNode> dependencies = new Stack<BaseNode>();

            dependencies.Push(node);

            while (dependencies.Count > 0)
            {
                var dependency = dependencies.Pop();

                var inputNodes = dependency.GetInputNodes();

                if (inputNodes!= null)
                {
                    foreach (var d in inputNodes.Where(n =>!(n is IConditionalNode)))
                        dependencies.Push(d);

                    if (dependency!= node)
                        yield return dependency;
                }
            }
        }

        private IEnumerator<BaseNode> RunTheGraph()
        {
            int count = processList.Count;

            for (int i = 0; i < count; i++)
            {
                processList[i].OnProcess();
                yield return processList[i];
            }
        }

        private IEnumerator<BaseNode> RunTheGraph(Stack<BaseNode> nodeToExecute)
        {
            HashSet<BaseNode> nodeDependenciesGathered = new HashSet<BaseNode>();
            HashSet<BaseNode> skipConditionalHandling = new HashSet<BaseNode>();

            while (nodeToExecute.Count > 0)
            {
                var node = nodeToExecute.Pop();
                if (node is IConditionalNode &&!skipConditionalHandling.Contains(node))
                {
                    if (nodeDependenciesGathered.Contains(node))
                    {
                        AllGraphWindow.HighLightNode(node);
                        node.OnProcess();
                        yield return node;

                        switch (node)
                        {
                            case ForLoopNode forLoopNode:
                                forLoopNode.index = forLoopNode.start - 1;
                                foreach (var n in forLoopNode.GetExecutedNodesLoopCompleted())
                                    nodeToExecute.Push(n);
                                for (int i = forLoopNode.start; i < forLoopNode.end; i++)
                                {
                                    foreach (var n in forLoopNode.GetExecutedNodesLoopBody())
                                        nodeToExecute.Push(n);

                                    nodeToExecute.Push(node);
                                }

                                skipConditionalHandling.Add(node);
                                break;
                            case WaitableNode waitableNode:
                                foreach (var n in waitableNode.GetExecutedNodes())
                                    nodeToExecute.Push(n);
                                currentWaitables.Add(waitableNode);
                                waitableNode.onProcessFinished += (waitedNode) =>
                                {
                                    currentWaitables.Remove(waitedNode);
                                    var netNodes = waitedNode.GetExecuteAfterNodes();
                                    if (netNodes!= null)
                                    {
                                        Stack<BaseNode> waitedNodes = new Stack<BaseNode>();
                                        foreach (var n in netNodes)
                                            waitedNodes.Push(n);
                                        WaitedRun(waitedNodes);
                                    }
                                    waitableNode.onProcessFinished = null;
                                };
                                break;
                            case IConditionalNode cNode:
                                var executs = cNode.GetExecutedNodes();
                                if (executs!= null && executs.Count() > 0)
                                    foreach (var n in executs)
                                    {
                                        nodeToExecute.Push(n);
                                    }
                                break;
                            default:
                                DebugEX.LogError($"Conditional node {node} not handled");
                                break;
                        }
                        nodeDependenciesGathered.Remove(node);
                    }
                    else
                    {
                        nodeToExecute.Push(node);
                        nodeDependenciesGathered.Add(node);
                        foreach (var nonConditionalNode in GatherNonConditionalDependencies(node))
                        {
                            nodeToExecute.Push(nonConditionalNode);
                        }
                    }
                }
                else
                {
#if UNITY_EDITOR
                    AllGraphWindow.HighLightNode(node);
#endif
                    node.OnProcess();
                    yield return node;
                }
            }
        }

        public void Step()
        {
            if (currentGraphExecution == null)
            {
                Stack<BaseNode> nodeToExecute = new Stack<BaseNode>();
                if (startNodeList.Count > 0)
                    startNodeList.ForEach(s => nodeToExecute.Push(s));

                currentGraphExecution = startNodeList.Count == 0? RunTheGraph() : RunTheGraph(nodeToExecute);
                currentGraphExecution.MoveNext(); // Advance to the first node
            }
            else
            if (!currentGraphExecution.MoveNext())
                currentGraphExecution = null;
        }
    }
}