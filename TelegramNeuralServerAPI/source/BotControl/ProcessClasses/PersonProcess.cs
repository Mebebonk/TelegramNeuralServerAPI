using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TelegramNeuralServerAPI
{
	internal class PersonProcess(BoundingBox boundingBox, Fitter? fitter, AgeEstimator? ageEstimator, LivenessEstimator? livenessEstimator, GenderEstimator? genderEstimator, EmotionEstimator? emotionEstimator, MaskEstimator? maskEstimator, GlassesEstimator? glassesEstimator, EyeOpennessEstimator? eyeOpennessEstimator)
	{

		[JsonInclude]
		[JsonPropertyName("boundingBox")]
		public BoundingBox boundingBox = boundingBox;

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
		[JsonPropertyName("EYE_OPENNESS_ESTIMATOR")]
		public EyeOpennessEstimator? eyeOpennessEstimator = eyeOpennessEstimator;

		[JsonInclude]
		[JsonPropertyName("GLASSES_ESTIMATOR")]
		public GlassesEstimator? glassesEstimator = glassesEstimator;

		public void WrappDescription(ImageInfo nfo)
		{
			nfo.TryAdd(ageEstimator?.ToString());
			nfo.TryAdd(livenessEstimator?.ToString());
			nfo.TryAdd(genderEstimator?.ToString());
			nfo.TryAdd(emotionEstimator?.ToString());
			nfo.TryAdd(maskEstimator?.ToString());
			nfo.TryAdd(eyeOpennessEstimator?.ToString());
			nfo.TryAdd(glassesEstimator?.ToString());
		}

		public bool IsFilled()
		{
			return ageEstimator is not null ||
				livenessEstimator is not null ||
				genderEstimator is not null ||
				emotionEstimator is not null ||
				maskEstimator is not null ||
				glassesEstimator is not null||
				eyeOpennessEstimator is not null;
		}
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
	internal readonly struct BoundingBox(Coordinate topLeft, Coordinate bottomRight)
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

		public override string ToString()
		{
			return $"Emotions:\n<pre>Angry: {angry}\nDisgusted: {disgusted}\nScared: {scared}\nHappy: {happy}\nNeutral: {neutral}\nSad: {sad}\nSurprised: {surprised}</pre>";
		}
	}

	[method: JsonConstructor]
	internal readonly struct AgeEstimator(int age)
	{
		[JsonInclude]
		[JsonPropertyName("age")]
		public readonly int age = age;

		public override string ToString()
		{
			return $"Age: <pre>Age: {age}</pre>";
		}
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
		public override string ToString()
		{
			return $"Liveness:\n<pre>Value: {value}\nConfidence: {confidence}</pre>";
		}
	}

	[method: JsonConstructor]
	internal readonly struct GenderEstimator(string gender)
	{
		[JsonInclude]
		[JsonPropertyName("gender")]
		public readonly string gender = gender;
		public override string ToString()
		{
			return $"Gender:\n<pre>Gender: {gender}</pre>";
		}
	}

	[method: JsonConstructor]
	internal readonly struct MaskEstimator(bool hasMask)
	{
		[JsonInclude]
		[JsonPropertyName("hasMask")]
		public readonly bool hasMask = hasMask;
		public override string ToString()
		{
			return $"Mask:\n<pre>Has mask: {hasMask}</>";
		}
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

		public override string ToString()
		{
			return $"Glasses:\n<pre>Has glasses: {hasGlasses}\nConfidence: {confidence}</pre>";
		}
	}

	[method: JsonConstructor]
	internal readonly struct EyeOpennessEstimator(bool isLeftEyeOpen, bool isRightEyeOpen, float leftEyeOpenConfidence, float rightEyeOpenConfidence)
	{
		[JsonInclude]
		[JsonPropertyName("isLeftEyeOpen")]
		public readonly bool isLeftEyeOpen = isLeftEyeOpen;
		[JsonInclude]
		[JsonPropertyName("isRightEyeOpen")]
		public readonly bool isRightEyeOpen = isRightEyeOpen;
		[JsonInclude]
		[JsonPropertyName("leftEyeOpenConfidence")]
		public readonly float leftEyeOpenConfidence = leftEyeOpenConfidence;
		[JsonInclude]
		[JsonPropertyName("rightEyeOpenConfidence")]
		public readonly float rightEyeOpenConfidence = rightEyeOpenConfidence;
		public override string ToString()
		{
			return $"Eye openness:\n<pre>Is left eye open: {isLeftEyeOpen}\nConfidence: {leftEyeOpenConfidence}\nIs right eye open: {isRightEyeOpen}\nConfidence: {rightEyeOpenConfidence}</pre>";
		}
	}
}
