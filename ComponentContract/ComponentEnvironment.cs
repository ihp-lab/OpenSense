namespace OpenSense.Component.Contract {

    public class ComponentEnvironment {

        public ComponentEnvironment(ComponentConfiguration configuration, object instance) {
            Configuration = configuration;
            Instance = instance;
        }

        public readonly ComponentConfiguration Configuration;

        public readonly object Instance;

    }
}
