# NQueenSolution
Efficient solver for all solutions for N-Queen Size boards

Solution uses:
1. State tracking so all allowable positions are validated without having to reanalyze previously checked validity
2. No additional test validation...all validation is incorporated into loops
3. Distinct Y-axis flip validation so only half the allowable solutions need be checked
