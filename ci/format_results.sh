#!/bin/bash

xml_file=$1

BOLD='\033[1;1m'
BRED='\033[1;31m'
BGREEN='\033[1;32m'
RED='\033[0;31m'
GREEN='\033[0;32m'
GREY='\033[0;90m'
NC='\033[0m'

# Task 1: Get the list of "classname" attribute values for all "test-suite" tags
classnames=$(xmlstarlet sel -t -m "//test-suite" -v "@classname" -n "$xml_file")

# Keep track of the total number of tests passed and failed, summing the list
total_passed=0
total_failed=0

# Task 2: For each classname, print its "fullname" attribute value and the number of tests passed
for classname in $classnames; do
    fullname=$(xmlstarlet sel -t -m "//test-suite[@classname='$classname']" -v "@fullname" -n "$xml_file")
    passed=$(xmlstarlet sel -t -m "//test-suite[@classname='$classname']" -v "@passed" -n "$xml_file")
    failed=$(xmlstarlet sel -t -m "//test-suite[@classname='$classname']" -v "@failed" -n "$xml_file")
    
    total_passed=$((total_passed + passed))
    total_failed=$((total_failed + failed))
    
    summary="$BGREEN${passed}~P/${failed}~F$NC"
    if [ "$failed" -ne 0 ]; then
        summary="$BGREEN${passed}~passed/$BRED${failed}~failed$NC"
    fi

    printf "$BOLD%-88s%s\n" "$fullname~" "~[$passed~passed~/~${failed}~failed]" | tr ' ~' '- '
    # Go through each "test-case" in the current test-suite, printing its name and status.
    # Underneath it, print any "output" elements.
    # If the test failed, also print the "failure" element.
    testcases=$(xmlstarlet sel -t -m "//test-suite[@classname='$classname']/test-case" -v "@name" -n "$xml_file")
    for testcase in $testcases; do
        status=$(xmlstarlet sel -t -m "//test-suite[@classname='$classname']/test-case[@name='$testcase']" -v "@result" -n "$xml_file")
        output=$(xmlstarlet sel -t -m "//test-suite[@classname='$classname']/test-case[@name='$testcase']/output" -v "." -n "$xml_file")
        failure_message=$(xmlstarlet sel -t -m "//test-suite[@classname='$classname']/test-case[@name='$testcase']/failure/message" -v "." -n "$xml_file")
        stack_trace=$(xmlstarlet sel -t -m "//test-suite[@classname='$classname']/test-case[@name='$testcase']/failure/stack-trace" -v "." -n "$xml_file")
        if [ "$status" = "Failed" ]; then
            printf "$RED%-80s%s\n" "$testcase~" "~$status" | tr ' ~' '- '
            printf "$GREY%s\n$NC" "$failure_message" | sed 's/^/    /'
            printf "$GREY%s\n$NC" "$stack_trace" | sed 's/^/    /'
        else
            printf "$GREEN%-80s%s\n$NC" "$testcase~" "~$status" | tr ' ~' '- '
        fi
        printf "$GREY%s$NC\n" "$output" | sed 's/^/    /'
    done
done

# Task 3: Print the total number of tests passed and failed overall (summed)
printf "${BOLD}Total: $total_passed tests passed, $total_failed tests failed$NC\n"