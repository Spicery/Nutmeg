package org.spicery.nutmeg.powerups.charrepeater;

public interface RecordingCharRepeaterInterface extends CharRepeaterInterface {
	
	void startRecording();
	
	String stopRecording();
	
	boolean isRecording();
	
	void backUp();
	
}
