import tensorflow as tf
import onnx
import keras2onnx

onnx_model_name = './model_for_conversion/emotion_model.onnx'

model = tf.keras.models.load_model('./model_for_conversion/emotion_model.hdf5')
onnx_model = keras2onnx.convert_keras(model, model.name)
onnx.save_model(onnx_model, onnx_model_name)