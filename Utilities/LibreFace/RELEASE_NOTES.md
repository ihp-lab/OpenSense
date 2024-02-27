# 2.0.0

This version mainly has two changes.

The first change led to a breaking change.
In order to optimize performance, we modified the model structure by merging the original AU Intensity detection model and the AU Presence detection model to some extent, changing from the original two larger models to now one larger model plus two smaller models.
The larger model takes images as inputs, while the two smaller models are responsible for outputing AU Intensity and AU Presence.
The output of the larger model will serve as the input for the smaller models.
When there is no need for either AU Intensity or AU Presence, the corresponding smaller model can be omitted.
This change requires adjustments to the invocation site.
The Facial Expression detection model is not affected by this change.

The second change is that we have enhanced the training data, so we expect this version to be adaptable to more application scenarios.

# 1.1.2

Fixed a bug that prevented the use of other providers.

# 1.1.1

Decouple ONNX runtime.
This allows for the use of different builds of ONNX runtime.

# 1.1.0

Include raw values in Action Unit presence outputs for debugging.

# 1.0.0

Initial Release