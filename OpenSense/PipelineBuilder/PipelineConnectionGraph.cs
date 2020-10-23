using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.MediaFoundation;

namespace OpenSense.PipelineBuilder {
    public class PipelineConnectionGraph {

        private PipelineEnvironment Pipeline;

        private Dictionary<Guid, int> Indices;

        private Dictionary<Guid, InstanceEnvironment> Lookup;

        private Dictionary<Guid, List<InstanceEnvironment>> Ancestors;

        private Dictionary<Guid, List<InstanceEnvironment>> Children;

        private List<InstanceEnvironment> Roots;

        public PipelineConnectionGraph(PipelineEnvironment pipeline) {
            Pipeline = pipeline;

            Indices = new Dictionary<Guid, int>();
            for (var i = 0; i < Pipeline.Instances.Count; i++) {
                Indices[Pipeline.Instances[i].Configuration.Guid] = i;
            }

            Lookup = new Dictionary<Guid, InstanceEnvironment>();
            foreach (var inst in Pipeline.Instances) {
                Lookup[inst.Configuration.Guid] = inst;
            }

            Ancestors = new Dictionary<Guid, List<InstanceEnvironment>>();
            foreach (var inst in Pipeline.Instances) {
                var conf = inst.Configuration;
                Ancestors[inst.Configuration.Guid] = inst.Configuration.Inputs.Select(i => i.Remote).Distinct().Select(i => Lookup[i]).ToList();
            }

            Children = new Dictionary<Guid, List<InstanceEnvironment>>();
            foreach (var inst in Pipeline.Instances) {
                Children[inst.Configuration.Guid] = Pipeline.Instances.Where(i => i.Configuration.Inputs.Select(ii => ii.Remote).Contains(inst.Configuration.Guid)).ToList();
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
            var ancestorHierachies = ancestors.Select(a => CalcHierachy(a.Configuration.Guid, cache)).ToList();
            var hierachy = ancestorHierachies.Max() + 1;
            cache[current] = hierachy;
            return hierachy;
        }

        private int CalcOffset(Guid current, Dictionary<Guid, int> cache) {
            if (cache.TryGetValue(current, out var result)) {
                return result;
            }
            var ancestors = Ancestors[current];
            List<InstanceEnvironment> siblings;
            if (ancestors.Count == 0) {
                result = 0;
                siblings = Roots;
            } else {
                var dominateAncestor = ancestors.OrderBy(a => Indices[a.Configuration.Guid]).First();
                result = CalcOffset(dominateAncestor.Configuration.Guid, cache);
                siblings = Children[dominateAncestor.Configuration.Guid];
            }
            var highPrioritySiblings = siblings.Where(s => Indices[s.Configuration.Guid] < Indices[current]).ToList();
            if (highPrioritySiblings.Count == 0) {
                cache[current] = result;
                return result;
            }
            var dominateSibling = highPrioritySiblings.OrderByDescending(s => Indices[s.Configuration.Guid]).First();
            int calcSpan(InstanceEnvironment inst) {
                var children = Children[inst.Configuration.Guid].Where(ch => inst == Ancestors[ch.Configuration.Guid].OrderBy(a => Indices[a.Configuration.Guid]).First()).ToList();
                return children.Count == 0 ? 1 : children.Sum(ch => calcSpan(ch));
            }
            result = CalcOffset(dominateSibling.Configuration.Guid, cache) + (calcSpan(dominateSibling) - 1) + 1;
            cache[current] = result;
            return result;
        }

        public Dictionary<Guid, Position> CalcPositions() {
            var result = new Dictionary<Guid, Position>();
            var hierachies = new Dictionary<Guid, int>();
            var offsets = new Dictionary<Guid, int>();
            foreach (var inst in Pipeline.Instances) {
                var hierachy = CalcHierachy(inst.Configuration.Guid, hierachies);
                var offset = CalcOffset(inst.Configuration.Guid, offsets);
                result[inst.Configuration.Guid] = new Position() { Hierachy = hierachy, Offset = offset };
            }
            return result;
        }

    }
}
