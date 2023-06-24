package org.spicery.nutmeg.powerups.charrepeater;

public class RecordingCharRepeater implements RecordingCharRepeaterInterface {

	boolean is_recording = false;
	StringBuilder builder = new StringBuilder();
	int end_of_file_count = 0;
	CharRepeaterInterface rep;

	public RecordingCharRepeater( CharRepeaterInterface rep ) {
		this.rep = rep;
	}

	public boolean hasNextChar() {
		return rep.hasNextChar();
	}

	public boolean isNextChar( char wanted ) {
		return rep.isNextChar( wanted );
	}

	public boolean isNextString( String wanted ) {
		return rep.isNextString( wanted );
	}

	public char nextChar() {
		char ch = rep.nextChar();
		if (is_recording) {
			pushToRecording( ch );
		}
		return ch;
	}

	public char nextChar( char value_if_needed ) {
		if (rep.hasNextChar()) {
			return this.nextChar();
		} else {
			this.end_of_file_count += 1;
			return value_if_needed;
		}
	}

	public void pushChar( char value ) {
		if (is_recording) {
			char ch = this.popFromRecording();
			if (ch == value) {
				rep.pushChar( value );
			} else {
				throw new RuntimeException("Trying to push back different character while recording is on");
			}
		} else {
			rep.pushChar( value );
		}
	}

	public char peekChar() {
		return rep.peekChar();
	}

	public char peekChar( char value_if_needed ) {
		return rep.peekChar( value_if_needed );
	}

	public void skipChar() {
		if (rep.hasNextChar()) {
			if (is_recording) {
				char ch = rep.nextChar();
				pushToRecording( ch );
			} else {
				rep.skipChar();
			}
		} else {
			this.end_of_file_count += 1;
		}
	}

	@Override
	public boolean isRecording() {
		return this.is_recording;
	}

	@Override
	public void startRecording() {
		this.is_recording = true;
		this.end_of_file_count = 0;
		this.clearRecording();
	}

	@Override
	public String stopRecording() {
		String s = this.builder.toString();
		this.is_recording = false;
		this.end_of_file_count = 0;
		this.clearRecording();
		return s;
	}
	
	@Override
	public void backUp() {
		if (this.end_of_file_count > 0) {
			this.end_of_file_count -= 1;
		} else {
			rep.pushChar( this.popFromRecording() );
		}
	}
	
	private void pushToRecording( char ch ) {
		this.builder.append( ch );
	}

	private char popFromRecording() {
		int L1 = this.builder.length() - 1;
		char ch = this.builder.charAt( L1 );
		this.builder.deleteCharAt( L1 );
		return ch;
	}

	private void clearRecording() {
		this.builder.setLength( 0 );
	}
}
