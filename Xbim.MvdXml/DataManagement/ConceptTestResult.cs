namespace Xbim.MvdXml.DataManagement
{
    /// <summary>
    /// contains the result of a concept being tested
    /// </summary>
    public enum ConceptTestResult
    {
        /// <summary>
        /// The specified concept does not apply to the element/s provided
        /// </summary>
        DoesNotApply = 0,
        /// <summary>
        /// the element/s provided comply with the concept requirements
        /// </summary>
        Pass = 1,
        /// <summary>
        /// Validation of the element/s provided results in warnings (see RequirementsRequirement test function)
        /// </summary>
        Warning = 2,
        /// <summary>
        /// the element/s provided do not comply with the concept requirements
        /// </summary>
        Fail = 3
    }
}
