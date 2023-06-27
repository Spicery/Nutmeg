package org.spicery.nutmeg.powerups.common;
/**
 * Copyright Stephen Leach, 2014
 * This file is part of the MinXML for Java library.
 * 
 * MinXML for Java is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Foobar is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with MinXML for Java.  If not, see <http://www.gnu.org/licenses/>.
 *  
 */


public class IfNull {

	/**
	 * A utility method that implements the idiom x != null ? x : y (i.e. not-null-OR)
	 * @param x the first value
	 * @param y the second value
	 * @return the first non-null value or null if all values are null.
	 * @param <T> the return type
	 */
	public static <T> T ifNull( final T x, final T y ) {
		return x == null ? y : x;
	}
	
	
}
