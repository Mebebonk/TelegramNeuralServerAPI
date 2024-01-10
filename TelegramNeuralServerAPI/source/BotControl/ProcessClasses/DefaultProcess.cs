using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class Person(FaceDetector faceDetector, Fitter? fitter, AgeEstimator? ageEstimator, LivenessEstimator? livenessEstimator, GenderEstimator? genderEstimator, EmotionEstimator? emotionEstimator, MaskEstimator? maskEstimator, GlassesEstimator? glassesEstimator)
	{

		[JsonInclude]
		[JsonPropertyName("boundingBox")]
		public FaceDetector faceDetector = faceDetector;

		[JsonInclude]
		[JsonPropertyName("FITTER")]
		public Fitter? fitter = fitter;

		[JsonInclude]
		[JsonPropertyName("AGE_ESTIMATOR")]
		public AgeEstimator? ageEstimator = ageEstimator;

		[JsonInclude]
		[JsonPropertyName("LIVENESS_ESTIMATOR")]
		public LivenessEstimator? livenessEstimator = livenessEstimator;

		[JsonInclude]
		[JsonPropertyName("GENDER_ESTIMATOR")]
		public GenderEstimator? genderEstimator = genderEstimator;

		[JsonInclude]
		[JsonPropertyName("EMOTION_ESTIMATOR")]
		public EmotionEstimator? emotionEstimator = emotionEstimator;

		[JsonInclude]
		[JsonPropertyName("MASK_ESTIMATOR")]
		public MaskEstimator? maskEstimator = maskEstimator;

		[JsonInclude]
		[JsonPropertyName("GLASSES_ESTIMATOR")]
		public GlassesEstimator? glassesEstimator = glassesEstimator;
	}

	[method: JsonConstructor]
	internal struct Coordinate(int x, int y)
	{
		[JsonInclude]
		[JsonPropertyName("x")]
		public int x = x;
		[JsonInclude]
		[JsonPropertyName("y")]
		public int y = y;
	}

	[method: JsonConstructor]
	internal readonly struct FaceDetector(Coordinate topLeft, Coordinate bottomRight)
	{
		[JsonInclude]
		[JsonPropertyName("topLeft")]
		public readonly Coordinate topLeft = topLeft;
		[JsonInclude]
		[JsonPropertyName("bottomRight")]
		public readonly Coordinate bottomRight = bottomRight;
	}

	[method: JsonConstructor]
	internal readonly struct Fitter(Coordinate leftEye, Coordinate rightEye, Coordinate mouth, Coordinate[] keypoints)
	{
		[JsonInclude][JsonPropertyName("left_eye")] public readonly Coordinate leftEye = leftEye;
		[JsonInclude][JsonPropertyName("right_eye")] public readonly Coordinate rightEye = rightEye;
		[JsonInclude][JsonPropertyName("mouth")] public readonly Coordinate mouth = mouth;

		[JsonInclude][JsonPropertyName("keypoints")] public readonly Coordinate[] keypoints = keypoints;
	}

	[method: JsonConstructor]
	internal readonly struct EmotionEstimator(float angry, float disgusted, float scared, float happy, float neutral, float sad, float surprised)
	{
		[JsonInclude][JsonPropertyName("ANGRY")] public readonly float angry = angry;
		[JsonInclude][JsonPropertyName("DISGUSTED")] public readonly float disgusted = disgusted;
		[JsonInclude][JsonPropertyName("SCARED")] public readonly float scared = scared;
		[JsonInclude][JsonPropertyName("HAPPY")] public readonly float happy = happy;
		[JsonInclude][JsonPropertyName("NEUTRAL")] public readonly float neutral = neutral;
		[JsonInclude][JsonPropertyName("SAD")] public readonly float sad = sad;
		[JsonInclude][JsonPropertyName("SURPRISED")] public readonly float surprised = surprised;
	}

	[method: JsonConstructor]
	internal readonly struct AgeEstimator(int age)
	{
		[JsonInclude]
		[JsonPropertyName("age")]
		public readonly int age = age;
	}

	[method: JsonConstructor]
	internal readonly struct LivenessEstimator(float confidence, string value)
	{
		[JsonInclude]
		[JsonPropertyName("confidence")]
		public readonly float confidence = confidence;
		[JsonInclude]
		[JsonPropertyName("value")]
		public readonly string value = value;
	}

	[method: JsonConstructor]
	internal readonly struct GenderEstimator(string gender)
	{
		[JsonInclude]
		[JsonPropertyName("gender")]
		public readonly string gender = gender;
	}

	[method: JsonConstructor]
	internal readonly struct MaskEstimator(bool hasMask)
	{
		[JsonInclude]
		[JsonPropertyName("hasMask")]
		public readonly bool hasMask = hasMask;
	}

	[method: JsonConstructor]
	internal readonly struct GlassesEstimator(bool hasGlasses, float confidence)
	{
		[JsonInclude]
		[JsonPropertyName("hasGlasses")]
		public readonly bool hasGlasses = hasGlasses;
		[JsonInclude]
		[JsonPropertyName("confidence")]
		public readonly float confidence = confidence;
	}
}
