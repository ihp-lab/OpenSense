using System;
using System.Collections.Generic;
using System.Linq;
using OpenSense.Components.Contract;
using OpenSense.Pipeline;

namespace OpenSense.WPF.Pipeline {
    public class PipelineConnectionGraph {

        private PipelineEnvironment Pipeline;

        private Dictionary<Guid, int> Indices;

        private Dictionary<Guid, ComponentEnvironment> Lookup;

        private Dictionary<Guid, List<ComponentEnvironment>> Ancestors;

        private Dictionary<Guid, List<ComponentEnvironment>> Children;

        private List<ComponentEnvironment> Roots;

        public PipelineConnectionGraph(PipelineEnvironment pipeline) {
            Pipeline = pipeline;

            Indices = new Dictionary<Guid, int>();
            for (var i = 0; i < Pipeline.Instances.Count; i++) {
                Indices[Pipeline.Instances[i].Configuration.Id] = i;
            }

            Lookup = new Dictionary<Guid, ComponentEnvironment>();
            foreach (var inst in Pipeline.Instances) {
                Lookup[inst.Configuration.Id] = inst;
            }

            Ancestors = new Dictionary<Guid, List<ComponentEnvironment>>();
            foreach (var inst in Pipeline.Instances) {
                var conf = inst.Configuration;
                Ancestors[inst.Configuration.Id] = inst.Configuration.Inputs.Select(i => i.RemoteId).Distinct().Select(i => Lookup[i]).ToList();
            }

            Children = new Dictionary<Guid, List<ComponentEnvironment>>();
            foreach (var inst in Pipeline.Instances) {
                Children[inst.Configuration.Id] = Pipeline.Instances.Where(i => i.Configuration.Inputs.Select(ii => ii.RemoteId).Contains(inst.Configuration.Id)).ToList();
            }

            Roots = Ancestors.Where(p => p.Value.Count == 0).Select(p => p.Key).Select(g => Lookup[g]).ToList();

        }

        public struct Position {
            public int Hierachy;
            public int Offset;
        }

        private int CalcHierachy(Guid current, Dictionary<Guid, int> cache) {
            if (cache.TryGetValue(current, out var result)) {
                return result;
            }
            var ancestors = Ancestors[current];
            if (ancestors.Count == 0) {
                cache[current] = 0;
                return 0;
            }
            var ancestorHierachies = ancestors.Select(a => CalcHierachy(a.Configuration.Id, cache)).ToList();
            var hierachy = ancestorHierachies.Max() + 1;
            cache[current] = hierachy;
            return hierachy;
        }

        private int CalcOffset(Guid current, Dictionary<Guid, int> cache) {
            if (cache.TryGetValue(current, out var result)) {
                return result;
            }
            var ancestors = Ancestors[current];
            List<ComponentEnvironment> siblings;
            if (ancestors.Count == 0) {
                result = 0;
                siblings = Roots;
            } else {
                var dominateAncestor = ancestors.OrderBy(a => Indices[a.Configuration.Id]).First();
                result = CalcOffset(dominateAncestor.Configuration.Id, cache);
                siblings = Children[dominateAncestor.Configuration.Id];
            }
            var highPrioritySiblings = siblings.Where(s => Indices[s.Configuration.Id] < Indices[current]).ToList();
            if (highPrioritySiblings.Count == 0) {
                cache[current] = result;
                return result;
            }
            var dominateSibling = highPrioritySiblings.OrderByDescending(s => Indices[s.Configuration.Id]).First();
            int calcSpan(ComponentEnvironment inst) {
                var children = Children[inst.Configuration.Id].Where(ch => inst == Ancestors[ch.Configuration.Id].OrderBy(a => Indices[a.Configuration.Id]).First()).ToList();
                return children.Count == 0 ? 1 : children.Sum(ch => calcSpan(ch));
            }
            result = CalcOffset(dominateSibling.Configuration.Id, cache) + (calcSpan(dominateSibling) - 1) + 1;
            cache[current] = result;
            return result;
        }

        public Dictionary<Guid, Position> CalcPositions() {
            var result = new Dictionary<Guid, Position>();
            var hierachies = new Dictionary<Guid, int>();
            var offsets = new Dictionary<Guid, int>();
            foreach (var inst in Pipeline.Instances) {
                var hierachy = CalcHierachy(inst.Configuration.Id, hierachies);
                var offset = CalcOffset(inst.Configuration.Id, offsets);
                result[inst.Configuration.Id] = new Position() { Hierachy = hierachy, Offset = offset };
            }
            return result;
        }

    }
}
